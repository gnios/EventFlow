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
    /// Base interface for builders that allow actions after an event.
    /// Defines the correct order: first publish commands, then emit events, then complete.
    /// This interface ensures the developer follows the correct path when building the saga.
    /// </summary>
    public interface IWhenBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>
        where TSaga : DeclarativeSaga<TSaga, TIdentity, TLocator>
        where TIdentity : ISagaId
        where TLocator : ISagaLocator
        where TAggregate : IAggregateRoot<TAggregateIdentity>
        where TAggregateIdentity : IIdentity
        where TAggregateEvent : IAggregateEvent<TAggregate, TAggregateIdentity>
    {
        /// <summary>
        /// Then publishes a command when the event occurs.
        /// ORDER: Use this method FIRST to publish commands.
        /// Returns IAfterPublishBuilder to continue the chain correctly.
        /// </summary>
        IAfterPublishBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent> ThenPublish<TCommandAggregate, TCommandAggregateIdentity>(
            Func<IDomainEvent<TAggregate, TAggregateIdentity, TAggregateEvent>, TSaga, ICommand<TCommandAggregate, TCommandAggregateIdentity, IExecutionResult>> commandFactory)
            where TCommandAggregate : IAggregateRoot<TCommandAggregateIdentity>
            where TCommandAggregateIdentity : IIdentity;

        /// <summary>
        /// Then publishes multiple commands when the event occurs.
        /// ORDER: Use this method FIRST to publish multiple commands.
        /// Returns IAfterPublishBuilder to continue the chain correctly.
        /// </summary>
        IAfterPublishBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent> ThenPublishMany<TCommandAggregate, TCommandAggregateIdentity>(
            Func<IDomainEvent<TAggregate, TAggregateIdentity, TAggregateEvent>, TSaga, IEnumerable<ICommand<TCommandAggregate, TCommandAggregateIdentity, IExecutionResult>>> commandsFactory)
            where TCommandAggregate : IAggregateRoot<TCommandAggregateIdentity>
            where TCommandAggregateIdentity : IIdentity;

        /// <summary>
        /// Then emits an internal saga event when the event occurs.
        /// ORDER: Use this method AFTER publishing commands to record saga facts.
        /// Returns IAfterEmitBuilder to continue the chain correctly.
        /// </summary>
        IAfterEmitBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent> ThenEmitSagaEvent<TSagaEvent>(
            Func<IDomainEvent<TAggregate, TAggregateIdentity, TAggregateEvent>, TSaga, TSagaEvent> eventFactory)
            where TSagaEvent : IAggregateEvent<TSaga, TIdentity>;

        /// <summary>
        /// Then schedules a command for future execution.
        /// ORDER: Use this method to schedule commands that will be executed in the future.
        /// Returns IAfterScheduleBuilder to continue the chain correctly.
        /// </summary>
        IAfterScheduleBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent> ThenSchedule<TCommandAggregate, TCommandAggregateIdentity>(
            Func<IDomainEvent<TAggregate, TAggregateIdentity, TAggregateEvent>, TSaga, ICommand<TCommandAggregate, TCommandAggregateIdentity, IExecutionResult>> commandFactory,
            TimeSpan delay)
            where TCommandAggregate : IAggregateRoot<TCommandAggregateIdentity>
            where TCommandAggregateIdentity : IIdentity;

        /// <summary>
        /// Then configures a timeout to wait for the next event.
        /// ORDER: Use this method AFTER publishing commands to wait for the next event.
        /// Returns ITimeoutBuilder which REQUIRES compensation definition.
        /// </summary>
        ITimeoutBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent> ThenTimeoutAfter(TimeSpan timeout);

        /// <summary>
        /// Then completes the saga when the event occurs.
        /// ORDER: Use this method LAST to finalize the saga.
        /// </summary>
        void ThenComplete();
    }
}

