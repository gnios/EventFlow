// The MIT License (MIT)
// 
// Copyright (c) 2015-2025 Rasmus Mikkelsen
// https://github.com/eventflow/EventFlow
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using EventFlow.Aggregates;
using EventFlow.Aggregates.ExecutionResults;
using EventFlow.Commands;
using EventFlow.Core;
using EventFlow.DeclarativeSaga.StateMachine.Builders;
using EventFlow.DeclarativeSaga.StateMachine.Events;
using EventFlow.Jobs;
using EventFlow.Provided.Jobs;
using EventFlow.Sagas;
using EventFlow.Sagas.AggregateSagas;
using Microsoft.Extensions.DependencyInjection;

namespace EventFlow.DeclarativeSaga.StateMachine
{
    /// <summary>
    /// Base class for creating declarative sagas using a fluent API similar to MassTransit and WorkflowCore.
    /// This class maintains the declarative API while using the standard EventFlow pattern for state management.
    /// 
    /// The saga is a standard AggregateSaga that implements ISagaIsStartedBy and ISagaHandles.
    /// State is managed through normal saga events (event sourcing), just like standard EventFlow sagas.
    /// 
    /// The builder uses Interface Segregation to guide developers through valid saga configurations,
    /// preventing invalid builder chains at compile time.
    /// 
    /// Usage example:
    /// public class MySaga : DeclarativeSaga&lt;MySaga, MySagaId, MySagaLocator&gt;,
    ///     ISagaIsStartedBy&lt;MyAggregate, MyId, MyStartedEvent&gt;,
    ///     ISagaHandles&lt;MyAggregate, MyId, MyCompletedEvent&gt;
    /// {
    ///     public MySaga(MySagaId id) : base(id)
    ///     {
    ///         Define(builder =>
    ///         {
    ///             builder.Initially()
    ///                 .When&lt;MyAggregate, MyId, MyStartedEvent&gt;()
    ///                 .ThenEmitSagaEvent((evt, saga) => new MySagaStartedEvent(...))
    ///                 .ThenPublish&lt;MyAggregate, MyId&gt;((evt, saga) => new MyCommand(...));
    ///             
    ///             builder.When&lt;MyAggregate, MyId, MyCompletedEvent&gt;()
    ///                 .ThenComplete();
    ///         });
    ///     }
    ///     
    ///     public Task HandleAsync(IDomainEvent&lt;MyAggregate, MyId, MyStartedEvent&gt; e, ISagaContext ctx, CancellationToken ct)
    ///         => HandleEventAsync(e, ctx, ct);
    ///     
    ///     public Task HandleAsync(IDomainEvent&lt;MyAggregate, MyId, MyCompletedEvent&gt; e, ISagaContext ctx, CancellationToken ct)
    ///         => HandleEventAsync(e, ctx, ct);
    /// }
    /// </summary>
    /// <typeparam name="TSaga">The saga type</typeparam>
    /// <typeparam name="TIdentity">The saga identity type</typeparam>
    /// <typeparam name="TLocator">The saga locator type</typeparam>
    public abstract class DeclarativeSaga<TSaga, TIdentity, TLocator> : AggregateSaga<TSaga, TIdentity, TLocator>
        where TSaga : DeclarativeSaga<TSaga, TIdentity, TLocator>
        where TIdentity : ISagaId
        where TLocator : ISagaLocator
    {
        private readonly Dictionary<Type, EventHandler> _eventHandlers = new Dictionary<Type, EventHandler>();
        private bool _definitionInitialized;
        private readonly ICommandScheduler? _commandScheduler;
        private readonly IJobScheduler? _jobScheduler;
        private readonly IServiceProvider? _serviceProvider;
        private IHasCompensationJobId? _compensationJobIdState;

        protected DeclarativeSaga(TIdentity id) : base(id)
        {
        }

        protected DeclarativeSaga(TIdentity id, ICommandScheduler? commandScheduler, IJobScheduler? jobScheduler) : base(id)
        {
            _commandScheduler = commandScheduler;
            _jobScheduler = jobScheduler;
        }

        protected DeclarativeSaga(TIdentity id, IServiceProvider serviceProvider) : base(id)
        {
            _serviceProvider = serviceProvider;
            _commandScheduler = serviceProvider?.GetService<ICommandScheduler>();
            _jobScheduler = serviceProvider?.GetService<IJobScheduler>();
        }

        /// <summary>
        /// Defines the saga configuration using the builder. Must be called in the constructor.
        /// This method creates a DeclarativeSagaBuilder instance and allows you to configure event handlers
        /// using the fluent API. The builder uses Interface Segregation to guide you through valid configurations.
        /// </summary>
        /// <param name="configure">Action that configures the saga using the builder</param>
        /// <exception cref="InvalidOperationException">Thrown if Define is called more than once</exception>
        /// <example>
        /// Define(builder =>
        /// {
        ///     builder.Initially()
        ///         .When&lt;OrderAggregate, OrderId, OrderCreatedEvent&gt;()
        ///         .ThenEmitSagaEvent((evt, saga) => new OrderSagaStartedEvent(...))
        ///         .ThenPublish&lt;OrderAggregate, OrderId&gt;((evt, saga) => new ReserveStockCommand(...));
        /// });
        /// </example>
        protected void Define(Action<DeclarativeSagaBuilder<TSaga, TIdentity, TLocator>> configure)
        {
            if (_definitionInitialized)
                throw new InvalidOperationException("Define can only be called once");

            var builder = new DeclarativeSagaBuilder<TSaga, TIdentity, TLocator>(_eventHandlers);
            configure(builder);
            _definitionInitialized = true;
        }

        /// <summary>
        /// Processes a domain event through the configured handlers.
        /// This method should be called in the HandleAsync methods of ISagaIsStartedBy and ISagaHandles interfaces.
        /// Automatically cancels any pending compensation job when the event is processed.
        /// </summary>
        /// <typeparam name="TAggregate">The aggregate type</typeparam>
        /// <typeparam name="TAggregateIdentity">The aggregate identity type</typeparam>
        /// <typeparam name="TAggregateEvent">The aggregate event type</typeparam>
        /// <param name="domainEvent">The domain event to process</param>
        /// <param name="sagaContext">The saga context</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the async operation</returns>
        /// <example>
        /// public Task HandleAsync(IDomainEvent&lt;OrderAggregate, OrderId, OrderCreatedEvent&gt; e, ISagaContext ctx, CancellationToken ct)
        ///     => HandleEventAsync(e, ctx, ct);
        /// </example>
        protected Task HandleEventAsync<TAggregate, TAggregateIdentity, TAggregateEvent>(
            IDomainEvent<TAggregate, TAggregateIdentity, TAggregateEvent> domainEvent,
            ISagaContext sagaContext,
            CancellationToken cancellationToken)
            where TAggregate : IAggregateRoot<TAggregateIdentity>
            where TAggregateIdentity : IIdentity
            where TAggregateEvent : IAggregateEvent<TAggregate, TAggregateIdentity>
        {
            var eventType = typeof(TAggregateEvent);
            if (_eventHandlers.TryGetValue(eventType, out var handler))
            {
                // Cancel compensation job before processing the event
                CancelCompensationJob();
                return handler.ExecuteAsync(domainEvent, (TSaga)(object)this, cancellationToken);
            }

            // If no handler found, do nothing (default behavior)
            return Task.CompletedTask;
        }

        /// <summary>
        /// Emits an internal saga event (helper method for the builder).
        /// This method is called internally by the builder when ThenEmitSagaEvent() is used.
        /// </summary>
        /// <typeparam name="TSagaEvent">The saga event type</typeparam>
        /// <param name="sagaEvent">The saga event to emit</param>
        internal void EmitSagaEvent<TSagaEvent>(TSagaEvent sagaEvent)
            where TSagaEvent : IAggregateEvent<TSaga, TIdentity>
        {
            Emit(sagaEvent);
        }

        /// <summary>
        /// Publishes a command (helper method for the builder).
        /// This method is called internally by the builder when ThenPublish() or ThenPublishMany() is used.
        /// </summary>
        /// <typeparam name="TCommandAggregate">The command aggregate type</typeparam>
        /// <typeparam name="TCommandAggregateIdentity">The command aggregate identity type</typeparam>
        /// <typeparam name="TExecutionResult">The execution result type</typeparam>
        /// <param name="command">The command to publish</param>
        internal void PublishCommand<TCommandAggregate, TCommandAggregateIdentity, TExecutionResult>(
            ICommand<TCommandAggregate, TCommandAggregateIdentity, TExecutionResult> command)
            where TCommandAggregate : IAggregateRoot<TCommandAggregateIdentity>
            where TCommandAggregateIdentity : IIdentity
            where TExecutionResult : IExecutionResult
        {
            Publish(command);
        }

        /// <summary>
        /// Schedules a command using reflection (helper method for the unified builder).
        /// This is an internal method used by the builder infrastructure.
        /// </summary>
        /// <param name="command">The command to schedule</param>
        /// <param name="delay">The delay before executing the command</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The job ID for cancellation purposes</returns>
        /// <exception cref="InvalidOperationException">Thrown if the command cannot be scheduled</exception>
        internal async Task<IJobId> ScheduleCommandWithJobIdAsyncDynamic(ICommand command, TimeSpan delay, CancellationToken cancellationToken)
        {
            var commandType = command.GetType();
            var commandInterface = commandType.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<,,>));
            
            if (commandInterface != null)
            {
                var genericArgs = commandInterface.GetGenericArguments();
                if (genericArgs.Length == 3)
                {
                    var method = typeof(DeclarativeSaga<TSaga, TIdentity, TLocator>).GetMethod(
                        "ScheduleCommandWithJobIdAsync",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                        null,
                        new[] { typeof(ICommand<,,>).MakeGenericType(genericArgs), typeof(TimeSpan), typeof(CancellationToken) },
                        null);
                    if (method != null)
                    {
                        var genericMethod = method.MakeGenericMethod(genericArgs[0], genericArgs[1], genericArgs[2]);
                        var task = (Task<IJobId>)genericMethod.Invoke(this, new object[] { command, delay, cancellationToken })!;
                        return await task.ConfigureAwait(false);
                    }
                }
            }
            throw new InvalidOperationException($"Could not schedule command {commandType.Name}");
        }

        /// <summary>
        /// Schedules a command for future execution using IJobScheduler (helper method for the builder).
        /// Returns the IJobId to allow later cancellation.
        /// This method is called internally by the builder when ThenSchedule() or ScheduleCommand() is used.
        /// </summary>
        /// <typeparam name="TCommandAggregate">The command aggregate type</typeparam>
        /// <typeparam name="TCommandAggregateIdentity">The command aggregate identity type</typeparam>
        /// <typeparam name="TExecutionResult">The execution result type</typeparam>
        /// <param name="command">The command to schedule</param>
        /// <param name="delay">The delay before executing the command</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The job ID for cancellation purposes</returns>
        /// <exception cref="InvalidOperationException">Thrown if IJobScheduler or IServiceProvider is not provided</exception>
        internal async Task<IJobId> ScheduleCommandWithJobIdAsync<TCommandAggregate, TCommandAggregateIdentity, TExecutionResult>(
            ICommand<TCommandAggregate, TCommandAggregateIdentity, TExecutionResult> command,
            TimeSpan delay,
            CancellationToken cancellationToken)
            where TCommandAggregate : IAggregateRoot<TCommandAggregateIdentity>
            where TCommandAggregateIdentity : IIdentity
            where TExecutionResult : IExecutionResult
        {
            if (_jobScheduler == null)
                throw new InvalidOperationException("IJobScheduler was not provided. Provide IJobScheduler in the saga constructor to use ScheduleCommand.");

            if (_serviceProvider == null)
                throw new InvalidOperationException("IServiceProvider was not provided. Use the constructor that accepts IServiceProvider to use ScheduleCommand.");

            // Creates a PublishCommandJob that will publish the command when executed
            var publishCommandJob = PublishCommandJob.Create(command, _serviceProvider);
            
            // Schedules the job and returns the ID to allow cancellation
            return await _jobScheduler.ScheduleAsync(publishCommandJob, delay, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Schedules a command for future execution at a specific date using IJobScheduler (helper method for the builder).
        /// Returns the IJobId to allow later cancellation.
        /// This method is called internally by the builder when ScheduleCommand() is used with a DateTimeOffset.
        /// </summary>
        /// <typeparam name="TCommandAggregate">The command aggregate type</typeparam>
        /// <typeparam name="TCommandAggregateIdentity">The command aggregate identity type</typeparam>
        /// <typeparam name="TExecutionResult">The execution result type</typeparam>
        /// <param name="command">The command to schedule</param>
        /// <param name="runAt">The specific date/time to execute the command</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The job ID for cancellation purposes</returns>
        /// <exception cref="InvalidOperationException">Thrown if IJobScheduler or IServiceProvider is not provided</exception>
        internal async Task<IJobId> ScheduleCommandWithJobIdAsync<TCommandAggregate, TCommandAggregateIdentity, TExecutionResult>(
            ICommand<TCommandAggregate, TCommandAggregateIdentity, TExecutionResult> command,
            DateTimeOffset runAt,
            CancellationToken cancellationToken)
            where TCommandAggregate : IAggregateRoot<TCommandAggregateIdentity>
            where TCommandAggregateIdentity : IIdentity
            where TExecutionResult : IExecutionResult
        {
            if (_jobScheduler == null)
                throw new InvalidOperationException("IJobScheduler was not provided. Provide IJobScheduler in the saga constructor to use ScheduleCommand.");

            if (_serviceProvider == null)
                throw new InvalidOperationException("IServiceProvider was not provided. Use the constructor that accepts IServiceProvider to use ScheduleCommand.");

            // Creates a PublishCommandJob that will publish the command when executed
            var publishCommandJob = PublishCommandJob.Create(command, _serviceProvider);
            
            // Schedules the job and returns the ID to allow cancellation
            return await _jobScheduler.ScheduleAsync(publishCommandJob, runAt, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Schedules a job for future execution (helper method for the builder).
        /// This method is used internally by the builder infrastructure.
        /// </summary>
        /// <param name="job">The job to schedule</param>
        /// <param name="delay">The delay before executing the job</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The job ID for cancellation purposes</returns>
        /// <exception cref="InvalidOperationException">Thrown if IJobScheduler is not provided</exception>
        internal async Task<IJobId> ScheduleJobAsync(
            IJob job,
            TimeSpan delay,
            CancellationToken cancellationToken)
        {
            if (_jobScheduler == null)
                throw new InvalidOperationException("IJobScheduler was not provided. Provide IJobScheduler in the saga constructor to use ScheduleJob.");

            return await _jobScheduler.ScheduleAsync(job, delay, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Schedules a job for future execution at a specific date (helper method for the builder).
        /// This method is used internally by the builder infrastructure.
        /// </summary>
        /// <param name="job">The job to schedule</param>
        /// <param name="runAt">The specific date/time to execute the job</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The job ID for cancellation purposes</returns>
        /// <exception cref="InvalidOperationException">Thrown if IJobScheduler is not provided</exception>
        internal async Task<IJobId> ScheduleJobAsync(
            IJob job,
            DateTimeOffset runAt,
            CancellationToken cancellationToken)
        {
            if (_jobScheduler == null)
                throw new InvalidOperationException("IJobScheduler was not provided. Provide IJobScheduler in the saga constructor to use ScheduleJob.");

            return await _jobScheduler.ScheduleAsync(job, runAt, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Completes the saga (helper method for the builder).
        /// This method is called internally by the builder when ThenComplete() or AndComplete() is used.
        /// Once completed, the saga will no longer process events.
        /// </summary>
        internal void CompleteSaga()
        {
            Complete();
        }

        /// <summary>
        /// Registers a saga state and automatically stores a reference if it implements IHasCompensationJobId.
        /// This method should be used instead of the base Register() method to enable automatic compensation job management.
        /// </summary>
        /// <typeparam name="TState">The state type</typeparam>
        /// <param name="state">The state instance to register</param>
        /// <returns>The registered state instance</returns>
        /// <example>
        /// public MySaga(MySagaId id, IServiceProvider serviceProvider) : base(id, serviceProvider)
        /// {
        ///     var state = RegisterState(new MySagaState());
        ///     // state is now registered and compensation job management is enabled
        /// }
        /// </example>
        protected TState RegisterState<TState>(TState state) where TState : IEventApplier<TSaga, TIdentity>
        {
            Register(state);
            
            // Automatically store reference if state implements IHasCompensationJobId
            if (state is IHasCompensationJobId hasCompensationJobId)
            {
                _compensationJobIdState = hasCompensationJobId;
            }
            
            return state;
        }

        /// <summary>
        /// Stores the compensation job ID for later cancellation.
        /// This method is called internally by the builder when compensation is defined for timeouts.
        /// The job ID is persisted through a saga event, ensuring resilience.
        /// The event itself contains the JobId, so no metadata is needed.
        /// </summary>
        /// <param name="jobId">The job ID to store</param>
        internal void StoreCompensationJobId(IJobId jobId)
        {
            // Simply emit the event - the JobId is already in the event payload
            // The state will apply this event and store the JobId
            Emit(new CompensationJobIdStoredEvent<TSaga, TIdentity>(jobId.Value));
        }

        /// <summary>
        /// Cancels the compensation job if it exists.
        /// This method is called automatically when an event is processed, ensuring that compensation
        /// jobs are cancelled if the expected event arrives before the timeout.
        /// The cancellation is persisted through a saga event, ensuring resilience.
        /// </summary>
        internal void CancelCompensationJob()
        {
            try
            {
                // Direct access to state - no reflection needed!
                if (_compensationJobIdState != null && !string.IsNullOrEmpty(_compensationJobIdState.CompensationJobId))
                {
                    var jobId = _compensationJobIdState.CompensationJobId;
                    
                    // Use Hangfire to cancel the job
                    var hangfireType = Type.GetType("Hangfire.BackgroundJob, Hangfire.Core");
                    if (hangfireType != null)
                    {
                        var deleteMethod = hangfireType.GetMethod("Delete", new[] { typeof(string) });
                        if (deleteMethod != null)
                        {
                            deleteMethod.Invoke(null, new object[] { jobId });
                        }
                    }
                    
                    // Emit event to clear the JobId (persisted)
                    Emit(new CompensationJobIdClearedEvent<TSaga, TIdentity>());
                }
            }
            catch
            {
                // Silently ignores cancellation errors
            }
        }

        /// <summary>
        /// Internal event handler class that stores and executes actions configured by the builder.
        /// This class is used internally by the DeclarativeSagaBuilder to store action chains
        /// that will be executed when events are processed.
        /// </summary>
        internal class EventHandler
        {
            private readonly List<Func<IDomainEvent, TSaga, CancellationToken, Task>> _actions = new List<Func<IDomainEvent, TSaga, CancellationToken, Task>>();
            private readonly List<Func<IDomainEvent, TSaga, CancellationToken, Task>> _actionsBefore = new List<Func<IDomainEvent, TSaga, CancellationToken, Task>>();

            /// <summary>
            /// Adds an action to be executed when the event is processed.
            /// Actions are executed in the order they are added.
            /// </summary>
            /// <param name="action">The action to execute</param>
            public void AddAction(Func<IDomainEvent, TSaga, CancellationToken, Task> action)
            {
                _actions.Add(action);
            }

            /// <summary>
            /// Adds an action to be executed before other actions when the event is processed.
            /// These actions are executed first, before the normal actions.
            /// </summary>
            /// <param name="action">The action to execute before others</param>
            public void AddActionBefore(Func<IDomainEvent, TSaga, CancellationToken, Task> action)
            {
                _actionsBefore.Add(action);
            }

            /// <summary>
            /// Executes all configured actions for the given domain event.
            /// First executes "before" actions, then normal actions, in the order they were added.
            /// </summary>
            /// <param name="domainEvent">The domain event to process</param>
            /// <param name="saga">The saga instance</param>
            /// <param name="cancellationToken">Cancellation token</param>
            /// <returns>Task representing the async operation</returns>
            public async Task ExecuteAsync(IDomainEvent domainEvent, TSaga saga, CancellationToken cancellationToken)
            {
                // Execute "before" actions first
                foreach (var action in _actionsBefore)
                {
                    await action(domainEvent, saga, cancellationToken).ConfigureAwait(false);
                }

                // Execute normal actions
                foreach (var action in _actions)
                {
                    await action(domainEvent, saga, cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }
}

