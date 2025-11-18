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
using EventFlow.Sagas;

namespace EventFlow.DeclarativeSaga.StateMachine.Builders.Interfaces
{
    /// <summary>
    /// Interface for builders after emitting saga events.
    /// ORDER: After emitting events, you can publish commands, schedule commands, configure timeout, or complete.
    /// This interface ensures the developer can only perform valid actions after emitting events.
    /// </summary>
    public interface IAfterEmitBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>
        where TSaga : DeclarativeSaga<TSaga, TIdentity, TLocator>
        where TIdentity : ISagaId
        where TLocator : ISagaLocator
        where TAggregate : IAggregateRoot<TAggregateIdentity>
        where TAggregateIdentity : IIdentity
        where TAggregateEvent : IAggregateEvent<TAggregate, TAggregateIdentity>
    {
        /// <summary>
        /// And publishes a command (Gherkin style).
        /// Allows publishing commands after emitting saga events.
        /// </summary>
        IAfterPublishBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent> AndPublish<TCommandAggregate, TCommandAggregateIdentity>(
            Func<IDomainEvent<TAggregate, TAggregateIdentity, TAggregateEvent>, TSaga, ICommand<TCommandAggregate, TCommandAggregateIdentity, IExecutionResult>> commandFactory)
            where TCommandAggregate : IAggregateRoot<TCommandAggregateIdentity>
            where TCommandAggregateIdentity : IIdentity;

        /// <summary>
        /// Then publishes a command.
        /// Allows publishing commands after emitting saga events.
        /// </summary>
        IAfterPublishBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent> ThenPublish<TCommandAggregate, TCommandAggregateIdentity>(
            Func<IDomainEvent<TAggregate, TAggregateIdentity, TAggregateEvent>, TSaga, ICommand<TCommandAggregate, TCommandAggregateIdentity, IExecutionResult>> commandFactory)
            where TCommandAggregate : IAggregateRoot<TCommandAggregateIdentity>
            where TCommandAggregateIdentity : IIdentity;

        /// <summary>
        /// And schedules a command for future execution (Gherkin style).
        /// </summary>
        IAfterScheduleBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent> AndSchedule<TCommandAggregate, TCommandAggregateIdentity>(
            Func<IDomainEvent<TAggregate, TAggregateIdentity, TAggregateEvent>, TSaga, ICommand<TCommandAggregate, TCommandAggregateIdentity, IExecutionResult>> commandFactory,
            TimeSpan delay)
            where TCommandAggregate : IAggregateRoot<TCommandAggregateIdentity>
            where TCommandAggregateIdentity : IIdentity;

        /// <summary>
        /// Then schedules a command for future execution.
        /// </summary>
        IAfterScheduleBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent> ThenSchedule<TCommandAggregate, TCommandAggregateIdentity>(
            Func<IDomainEvent<TAggregate, TAggregateIdentity, TAggregateEvent>, TSaga, ICommand<TCommandAggregate, TCommandAggregateIdentity, IExecutionResult>> commandFactory,
            TimeSpan delay)
            where TCommandAggregate : IAggregateRoot<TCommandAggregateIdentity>
            where TCommandAggregateIdentity : IIdentity;

        /// <summary>
        /// Then configures a timeout to wait for the next event.
        /// Returns ITimeoutBuilder which REQUIRES compensation definition.
        /// </summary>
        ITimeoutBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent> ThenTimeoutAfter(TimeSpan timeout);

        /// <summary>
        /// And completes the saga (Gherkin style).
        /// </summary>
        void AndComplete();

        /// <summary>
        /// Then completes the saga.
        /// </summary>
        void ThenComplete();
    }
}

