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
    /// Builder for configuring compensation after timeout (BDD style).
    /// Use this builder after calling ThenTimeoutAfter() to define the compensation command.
    /// This class implements Interface Segregation to REQUIRE compensation definition.
    /// </summary>
    public class TimeoutBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent> :
        ITimeoutBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>
        where TSaga : DeclarativeSaga<TSaga, TIdentity, TLocator>
        where TIdentity : ISagaId
        where TLocator : ISagaLocator
        where TAggregate : IAggregateRoot<TAggregateIdentity>
        where TAggregateIdentity : IIdentity
        where TAggregateEvent : IAggregateEvent<TAggregate, TAggregateIdentity>
    {
        private readonly DeclarativeSaga<TSaga, TIdentity, TLocator>.EventHandler _handler;
        private readonly TimeSpan _timeout;

        internal TimeoutBuilder(
            DeclarativeSaga<TSaga, TIdentity, TLocator>.EventHandler handler,
            TimeSpan timeout)
        {
            _handler = handler;
            _timeout = timeout;
        }

        /// <summary>
        /// And compensates with a command when timeout occurs (Gherkin style).
        /// If the expected event arrives before timeout, the compensation job will be automatically cancelled.
        /// ORDER: This method is REQUIRED after ThenTimeoutAfter.
        /// </summary>
        /// <example>
        /// builder.When&lt;OrderAggregate, OrderId, OrderStockReservedEvent&gt;()
        ///     .ThenPublish(...)
        ///     .ThenTimeoutAfter(TimeSpan.FromMinutes(10))
        ///     .AndCompensateWith&lt;OrderAggregate, OrderId&gt;((evt, saga) => new StockRollbackCommand(evt.AggregateIdentity));
        /// </example>
        public IAfterCompensateBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent> AndCompensateWith<TCompensationCommandAggregate, TCompensationCommandAggregateIdentity>(
            Func<IDomainEvent<TAggregate, TAggregateIdentity, TAggregateEvent>, TSaga, ICommand<TCompensationCommandAggregate, TCompensationCommandAggregateIdentity, IExecutionResult>> compensationCommandFactory)
            where TCompensationCommandAggregate : IAggregateRoot<TCompensationCommandAggregateIdentity>
            where TCompensationCommandAggregateIdentity : IIdentity
        {
            return ThenCompensateWith(compensationCommandFactory);
        }

        /// <summary>
        /// Then compensates with a command when timeout occurs (BDD style).
        /// If the expected event arrives before timeout, the compensation job will be automatically cancelled.
        /// ORDER: This method is REQUIRED after ThenTimeoutAfter.
        /// </summary>
        /// <example>
        /// builder.When&lt;OrderAggregate, OrderId, OrderStockReservedEvent&gt;()
        ///     .ThenPublish(...)
        ///     .ThenTimeoutAfter(TimeSpan.FromMinutes(10))
        ///     .ThenCompensateWith&lt;OrderAggregate, OrderId&gt;((evt, saga) => new StockRollbackCommand(evt.AggregateIdentity));
        /// </example>
        public IAfterCompensateBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent> ThenCompensateWith<TCompensationCommandAggregate, TCompensationCommandAggregateIdentity>(
            Func<IDomainEvent<TAggregate, TAggregateIdentity, TAggregateEvent>, TSaga, ICommand<TCompensationCommandAggregate, TCompensationCommandAggregateIdentity, IExecutionResult>> compensationCommandFactory)
            where TCompensationCommandAggregate : IAggregateRoot<TCompensationCommandAggregateIdentity>
            where TCompensationCommandAggregateIdentity : IIdentity
        {
            // Schedules the compensation job when this (previous) event occurs
            _handler.AddAction(async (domainEvent, saga, ct) =>
            {
                if (domainEvent is IDomainEvent<TAggregate, TAggregateIdentity, TAggregateEvent> typedEvent)
                {
                    var compensationCommand = compensationCommandFactory(typedEvent, saga);
                    var jobId = await saga.ScheduleCommandWithJobIdAsync(compensationCommand, _timeout, ct).ConfigureAwait(false);
                    
                    // Stores the jobId in the saga state for later cancellation
                    saga.StoreCompensationJobId(jobId);
                }
            });

            return new WhenBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>(_handler);
        }
    }
}

