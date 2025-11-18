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
using EventFlow.DeclarativeSaga.StateMachine.Builders.Interfaces;
using EventFlow.Sagas;

namespace EventFlow.DeclarativeSaga.StateMachine.Builders
{
    /// <summary>
    /// Builder for configuring actions when an event occurs.
    /// This class implements interfaces that force the correct order of methods (Interface Segregation).
    /// The developer can only call valid methods at each stage, preventing invalid builders.
    /// </summary>
    public class WhenBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent> :
        IWhenBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>,
        IAfterPublishBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>,
        IAfterEmitBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>,
        IAfterScheduleBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>,
        IAfterCompensateBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>
        where TSaga : DeclarativeSaga<TSaga, TIdentity, TLocator>
        where TIdentity : ISagaId
        where TLocator : ISagaLocator
        where TAggregate : IAggregateRoot<TAggregateIdentity>
        where TAggregateIdentity : IIdentity
        where TAggregateEvent : IAggregateEvent<TAggregate, TAggregateIdentity>
    {
        internal readonly DeclarativeSaga<TSaga, TIdentity, TLocator>.EventHandler _handler;

        internal WhenBuilder(DeclarativeSaga<TSaga, TIdentity, TLocator>.EventHandler handler)
        {
            _handler = handler;
        }

        // Public methods that delegate to interfaces - allows direct API usage
        public IAfterPublishBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent> ThenPublish<TCommandAggregate, TCommandAggregateIdentity>(
            Func<IDomainEvent<TAggregate, TAggregateIdentity, TAggregateEvent>, TSaga, ICommand<TCommandAggregate, TCommandAggregateIdentity, IExecutionResult>> commandFactory)
            where TCommandAggregate : IAggregateRoot<TCommandAggregateIdentity>
            where TCommandAggregateIdentity : IIdentity
        {
            return ((IWhenBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>)this).ThenPublish(commandFactory);
        }

        public IAfterPublishBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent> ThenPublishMany<TCommandAggregate, TCommandAggregateIdentity>(
            Func<IDomainEvent<TAggregate, TAggregateIdentity, TAggregateEvent>, TSaga, IEnumerable<ICommand<TCommandAggregate, TCommandAggregateIdentity, IExecutionResult>>> commandsFactory)
            where TCommandAggregate : IAggregateRoot<TCommandAggregateIdentity>
            where TCommandAggregateIdentity : IIdentity
        {
            return ((IWhenBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>)this).ThenPublishMany(commandsFactory);
        }

        public IAfterEmitBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent> ThenEmitSagaEvent<TSagaEvent>(
            Func<IDomainEvent<TAggregate, TAggregateIdentity, TAggregateEvent>, TSaga, TSagaEvent> eventFactory)
            where TSagaEvent : IAggregateEvent<TSaga, TIdentity>
        {
            return ((IWhenBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>)this).ThenEmitSagaEvent(eventFactory);
        }

        public IAfterScheduleBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent> ThenSchedule<TCommandAggregate, TCommandAggregateIdentity>(
            Func<IDomainEvent<TAggregate, TAggregateIdentity, TAggregateEvent>, TSaga, ICommand<TCommandAggregate, TCommandAggregateIdentity, IExecutionResult>> commandFactory,
            TimeSpan delay)
            where TCommandAggregate : IAggregateRoot<TCommandAggregateIdentity>
            where TCommandAggregateIdentity : IIdentity
        {
            return ((IWhenBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>)this).ThenSchedule(commandFactory, delay);
        }

        public ITimeoutBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent> ThenTimeoutAfter(TimeSpan timeout)
        {
            return ((IWhenBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>)this).ThenTimeoutAfter(timeout);
        }

        public void ThenComplete()
        {
            ((IWhenBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>)this).ThenComplete();
        }

        /// <summary>
        /// Executes a synchronous action when the event occurs.
        /// Use this method for simple actions that do not require asynchronous operations.
        /// </summary>
        /// <example>
        /// .Then((evt, saga) => {
        ///     saga._logger?.LogInformation("Event processed: {Event}", evt.AggregateEvent);
        /// })
        /// </example>
        public WhenBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent> Then(
            Action<IDomainEvent<TAggregate, TAggregateIdentity, TAggregateEvent>, TSaga> action)
        {
            _handler.AddAction((domainEvent, saga, ct) =>
            {
                if (domainEvent is IDomainEvent<TAggregate, TAggregateIdentity, TAggregateEvent> typedEvent)
                {
                    action(typedEvent, saga);
                }
                return Task.CompletedTask;
            });
            return this;
        }

        /// <summary>
        /// Executes an asynchronous action when the event occurs.
        /// Use this method for actions that require asynchronous operations (I/O, API calls, etc.).
        /// </summary>
        /// <example>
        /// .ThenAsync(async (evt, saga, ct) => {
        ///     await saga._service.ProcessAsync(evt.AggregateIdentity, ct);
        /// })
        /// </example>
        public WhenBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent> ThenAsync(
            Func<IDomainEvent<TAggregate, TAggregateIdentity, TAggregateEvent>, TSaga, CancellationToken, Task> action)
        {
            _handler.AddAction((domainEvent, saga, ct) =>
            {
                if (domainEvent is IDomainEvent<TAggregate, TAggregateIdentity, TAggregateEvent> typedEvent)
                {
                    return action(typedEvent, saga, ct);
                }
                return Task.CompletedTask;
            });
            return this;
        }

        /// <summary>
        /// Schedules a command for future execution at a specific date in Hangfire.
        /// Use this method to schedule commands that should be executed at a specific time.
        /// </summary>
        /// <example>
        /// .ScheduleCommand&lt;OrderAggregate, OrderId&gt;(
        ///     (evt, saga) => new SendReminderCommand(evt.AggregateIdentity),
        ///     DateTimeOffset.UtcNow.AddDays(7)
        /// )
        /// </example>
        public WhenBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent> ScheduleCommand<TCommandAggregate, TCommandAggregateIdentity>(
            Func<IDomainEvent<TAggregate, TAggregateIdentity, TAggregateEvent>, TSaga, ICommand<TCommandAggregate, TCommandAggregateIdentity, IExecutionResult>> commandFactory,
            DateTimeOffset runAt)
            where TCommandAggregate : IAggregateRoot<TCommandAggregateIdentity>
            where TCommandAggregateIdentity : IIdentity
        {
            _handler.AddAction(async (domainEvent, saga, ct) =>
            {
                if (domainEvent is IDomainEvent<TAggregate, TAggregateIdentity, TAggregateEvent> typedEvent)
                {
                    var command = commandFactory(typedEvent, saga);
                    await saga.ScheduleCommandWithJobIdAsync(command, runAt, ct).ConfigureAwait(false);
                }
            });
            return this;
        }

        // Implementation of IWhenBuilder.ThenPublish
        IAfterPublishBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent> IWhenBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>.ThenPublish<TCommandAggregate, TCommandAggregateIdentity>(
            Func<IDomainEvent<TAggregate, TAggregateIdentity, TAggregateEvent>, TSaga, ICommand<TCommandAggregate, TCommandAggregateIdentity, IExecutionResult>> commandFactory)
        {
            _handler.AddAction((domainEvent, saga, ct) =>
            {
                if (domainEvent is IDomainEvent<TAggregate, TAggregateIdentity, TAggregateEvent> typedEvent)
                {
                    var command = commandFactory(typedEvent, saga);
                    saga.PublishCommand(command);
                }
                return Task.CompletedTask;
            });
            return this;
        }

        // Implementation of IWhenBuilder.ThenPublishMany
        IAfterPublishBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent> IWhenBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>.ThenPublishMany<TCommandAggregate, TCommandAggregateIdentity>(
            Func<IDomainEvent<TAggregate, TAggregateIdentity, TAggregateEvent>, TSaga, IEnumerable<ICommand<TCommandAggregate, TCommandAggregateIdentity, IExecutionResult>>> commandsFactory)
        {
            _handler.AddAction((domainEvent, saga, ct) =>
            {
                if (domainEvent is IDomainEvent<TAggregate, TAggregateIdentity, TAggregateEvent> typedEvent)
                {
                    var commands = commandsFactory(typedEvent, saga);
                    foreach (var command in commands)
                    {
                        saga.PublishCommand(command);
                    }
                }
                return Task.CompletedTask;
            });
            return this;
        }

        // Implementation of IWhenBuilder.ThenEmitSagaEvent
        IAfterEmitBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent> IWhenBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>.ThenEmitSagaEvent<TSagaEvent>(
            Func<IDomainEvent<TAggregate, TAggregateIdentity, TAggregateEvent>, TSaga, TSagaEvent> eventFactory)
        {
            _handler.AddAction((domainEvent, saga, ct) =>
            {
                if (domainEvent is IDomainEvent<TAggregate, TAggregateIdentity, TAggregateEvent> typedEvent)
                {
                    var sagaEvent = eventFactory(typedEvent, saga);
                    saga.EmitSagaEvent(sagaEvent);
                }
                return Task.CompletedTask;
            });
            return this;
        }

        // Implementation of IWhenBuilder.ThenSchedule
        IAfterScheduleBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent> IWhenBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>.ThenSchedule<TCommandAggregate, TCommandAggregateIdentity>(
            Func<IDomainEvent<TAggregate, TAggregateIdentity, TAggregateEvent>, TSaga, ICommand<TCommandAggregate, TCommandAggregateIdentity, IExecutionResult>> commandFactory,
            TimeSpan delay)
        {
            _handler.AddAction(async (domainEvent, saga, ct) =>
            {
                if (domainEvent is IDomainEvent<TAggregate, TAggregateIdentity, TAggregateEvent> typedEvent)
                {
                    var command = commandFactory(typedEvent, saga);
                    await saga.ScheduleCommandWithJobIdAsync(command, delay, ct).ConfigureAwait(false);
                }
            });
            return this;
        }

        // Implementation of IWhenBuilder.ThenTimeoutAfter
        ITimeoutBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent> IWhenBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>.ThenTimeoutAfter(TimeSpan timeout)
        {
            return new TimeoutBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>(_handler, timeout);
        }

        // Implementation of IWhenBuilder.ThenComplete
        void IWhenBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>.ThenComplete()
        {
            _handler.AddAction((domainEvent, saga, ct) =>
            {
                saga.CompleteSaga();
                return Task.CompletedTask;
            });
        }

        // Implementations of IAfterPublishBuilder
        IAfterEmitBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent> IAfterPublishBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>.AndEmitSagaEvent<TSagaEvent>(
            Func<IDomainEvent<TAggregate, TAggregateIdentity, TAggregateEvent>, TSaga, TSagaEvent> eventFactory)
        {
            return ((IAfterPublishBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>)this).ThenEmitSagaEvent(eventFactory);
        }

        IAfterEmitBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent> IAfterPublishBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>.ThenEmitSagaEvent<TSagaEvent>(
            Func<IDomainEvent<TAggregate, TAggregateIdentity, TAggregateEvent>, TSaga, TSagaEvent> eventFactory)
        {
            return ((IWhenBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>)this).ThenEmitSagaEvent(eventFactory);
        }

        IAfterScheduleBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent> IAfterPublishBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>.AndSchedule<TCommandAggregate, TCommandAggregateIdentity>(
            Func<IDomainEvent<TAggregate, TAggregateIdentity, TAggregateEvent>, TSaga, ICommand<TCommandAggregate, TCommandAggregateIdentity, IExecutionResult>> commandFactory,
            TimeSpan delay)
        {
            return ((IAfterPublishBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>)this).ThenSchedule(commandFactory, delay);
        }

        IAfterScheduleBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent> IAfterPublishBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>.ThenSchedule<TCommandAggregate, TCommandAggregateIdentity>(
            Func<IDomainEvent<TAggregate, TAggregateIdentity, TAggregateEvent>, TSaga, ICommand<TCommandAggregate, TCommandAggregateIdentity, IExecutionResult>> commandFactory,
            TimeSpan delay)
        {
            return ((IWhenBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>)this).ThenSchedule(commandFactory, delay);
        }

        ITimeoutBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent> IAfterPublishBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>.ThenTimeoutAfter(TimeSpan timeout)
        {
            return ((IWhenBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>)this).ThenTimeoutAfter(timeout);
        }

        void IAfterPublishBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>.AndComplete()
        {
            ((IAfterPublishBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>)this).ThenComplete();
        }

        void IAfterPublishBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>.ThenComplete()
        {
            ((IWhenBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>)this).ThenComplete();
        }

        // Implementations of IAfterEmitBuilder
        IAfterPublishBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent> IAfterEmitBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>.AndPublish<TCommandAggregate, TCommandAggregateIdentity>(
            Func<IDomainEvent<TAggregate, TAggregateIdentity, TAggregateEvent>, TSaga, ICommand<TCommandAggregate, TCommandAggregateIdentity, IExecutionResult>> commandFactory)
        {
            return ((IAfterEmitBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>)this).ThenPublish(commandFactory);
        }

        IAfterPublishBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent> IAfterEmitBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>.ThenPublish<TCommandAggregate, TCommandAggregateIdentity>(
            Func<IDomainEvent<TAggregate, TAggregateIdentity, TAggregateEvent>, TSaga, ICommand<TCommandAggregate, TCommandAggregateIdentity, IExecutionResult>> commandFactory)
        {
            return ((IWhenBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>)this).ThenPublish(commandFactory);
        }

        IAfterScheduleBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent> IAfterEmitBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>.AndSchedule<TCommandAggregate, TCommandAggregateIdentity>(
            Func<IDomainEvent<TAggregate, TAggregateIdentity, TAggregateEvent>, TSaga, ICommand<TCommandAggregate, TCommandAggregateIdentity, IExecutionResult>> commandFactory,
            TimeSpan delay)
        {
            return ((IAfterEmitBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>)this).ThenSchedule(commandFactory, delay);
        }

        IAfterScheduleBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent> IAfterEmitBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>.ThenSchedule<TCommandAggregate, TCommandAggregateIdentity>(
            Func<IDomainEvent<TAggregate, TAggregateIdentity, TAggregateEvent>, TSaga, ICommand<TCommandAggregate, TCommandAggregateIdentity, IExecutionResult>> commandFactory,
            TimeSpan delay)
        {
            return ((IWhenBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>)this).ThenSchedule(commandFactory, delay);
        }

        ITimeoutBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent> IAfterEmitBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>.ThenTimeoutAfter(TimeSpan timeout)
        {
            return ((IWhenBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>)this).ThenTimeoutAfter(timeout);
        }

        void IAfterEmitBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>.AndComplete()
        {
            ((IAfterEmitBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>)this).ThenComplete();
        }

        void IAfterEmitBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>.ThenComplete()
        {
            ((IWhenBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>)this).ThenComplete();
        }

        // Implementations of IAfterScheduleBuilder
        IAfterEmitBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent> IAfterScheduleBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>.AndEmitSagaEvent<TSagaEvent>(
            Func<IDomainEvent<TAggregate, TAggregateIdentity, TAggregateEvent>, TSaga, TSagaEvent> eventFactory)
        {
            return ((IAfterScheduleBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>)this).ThenEmitSagaEvent(eventFactory);
        }

        IAfterEmitBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent> IAfterScheduleBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>.ThenEmitSagaEvent<TSagaEvent>(
            Func<IDomainEvent<TAggregate, TAggregateIdentity, TAggregateEvent>, TSaga, TSagaEvent> eventFactory)
        {
            return ((IWhenBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>)this).ThenEmitSagaEvent(eventFactory);
        }

        IAfterPublishBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent> IAfterScheduleBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>.AndPublish<TCommandAggregate, TCommandAggregateIdentity>(
            Func<IDomainEvent<TAggregate, TAggregateIdentity, TAggregateEvent>, TSaga, ICommand<TCommandAggregate, TCommandAggregateIdentity, IExecutionResult>> commandFactory)
        {
            return ((IAfterScheduleBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>)this).ThenPublish(commandFactory);
        }

        IAfterPublishBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent> IAfterScheduleBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>.ThenPublish<TCommandAggregate, TCommandAggregateIdentity>(
            Func<IDomainEvent<TAggregate, TAggregateIdentity, TAggregateEvent>, TSaga, ICommand<TCommandAggregate, TCommandAggregateIdentity, IExecutionResult>> commandFactory)
        {
            return ((IWhenBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>)this).ThenPublish(commandFactory);
        }

        void IAfterScheduleBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>.AndComplete()
        {
            ((IAfterScheduleBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>)this).ThenComplete();
        }

        void IAfterScheduleBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>.ThenComplete()
        {
            ((IWhenBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>)this).ThenComplete();
        }

        // Implementations of IAfterCompensateBuilder
        IAfterEmitBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent> IAfterCompensateBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>.AndEmitSagaEvent<TSagaEvent>(
            Func<IDomainEvent<TAggregate, TAggregateIdentity, TAggregateEvent>, TSaga, TSagaEvent> eventFactory)
        {
            return ((IAfterCompensateBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>)this).ThenEmitSagaEvent(eventFactory);
        }

        IAfterEmitBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent> IAfterCompensateBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>.ThenEmitSagaEvent<TSagaEvent>(
            Func<IDomainEvent<TAggregate, TAggregateIdentity, TAggregateEvent>, TSaga, TSagaEvent> eventFactory)
        {
            return ((IWhenBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>)this).ThenEmitSagaEvent(eventFactory);
        }

        IAfterPublishBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent> IAfterCompensateBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>.AndPublish<TCommandAggregate, TCommandAggregateIdentity>(
            Func<IDomainEvent<TAggregate, TAggregateIdentity, TAggregateEvent>, TSaga, ICommand<TCommandAggregate, TCommandAggregateIdentity, IExecutionResult>> commandFactory)
        {
            return ((IAfterCompensateBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>)this).ThenPublish(commandFactory);
        }

        IAfterPublishBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent> IAfterCompensateBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>.ThenPublish<TCommandAggregate, TCommandAggregateIdentity>(
            Func<IDomainEvent<TAggregate, TAggregateIdentity, TAggregateEvent>, TSaga, ICommand<TCommandAggregate, TCommandAggregateIdentity, IExecutionResult>> commandFactory)
        {
            return ((IWhenBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>)this).ThenPublish(commandFactory);
        }

        void IAfterCompensateBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>.AndComplete()
        {
            ((IAfterCompensateBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>)this).ThenComplete();
        }

        void IAfterCompensateBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>.ThenComplete()
        {
            ((IWhenBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>)this).ThenComplete();
        }
    }
}

