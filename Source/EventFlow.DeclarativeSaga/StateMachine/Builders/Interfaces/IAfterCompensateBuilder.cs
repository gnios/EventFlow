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
    /// Interface for builders after defining compensation.
    /// ORDER: After defining compensation, you can emit events, publish commands, or complete.
    /// This interface ensures the developer can only perform valid actions after defining compensation.
    /// </summary>
    public interface IAfterCompensateBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>
        where TSaga : DeclarativeSaga<TSaga, TIdentity, TLocator>
        where TIdentity : ISagaId
        where TLocator : ISagaLocator
        where TAggregate : IAggregateRoot<TAggregateIdentity>
        where TAggregateIdentity : IIdentity
        where TAggregateEvent : IAggregateEvent<TAggregate, TAggregateIdentity>
    {
        /// <summary>
        /// And emits an internal saga event (Gherkin style).
        /// </summary>
        IAfterEmitBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent> AndEmitSagaEvent<TSagaEvent>(
            Func<IDomainEvent<TAggregate, TAggregateIdentity, TAggregateEvent>, TSaga, TSagaEvent> eventFactory)
            where TSagaEvent : IAggregateEvent<TSaga, TIdentity>;

        /// <summary>
        /// Then emits an internal saga event.
        /// </summary>
        IAfterEmitBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent> ThenEmitSagaEvent<TSagaEvent>(
            Func<IDomainEvent<TAggregate, TAggregateIdentity, TAggregateEvent>, TSaga, TSagaEvent> eventFactory)
            where TSagaEvent : IAggregateEvent<TSaga, TIdentity>;

        /// <summary>
        /// And publishes a command (Gherkin style).
        /// </summary>
        IAfterPublishBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent> AndPublish<TCommandAggregate, TCommandAggregateIdentity>(
            Func<IDomainEvent<TAggregate, TAggregateIdentity, TAggregateEvent>, TSaga, ICommand<TCommandAggregate, TCommandAggregateIdentity, IExecutionResult>> commandFactory)
            where TCommandAggregate : IAggregateRoot<TCommandAggregateIdentity>
            where TCommandAggregateIdentity : IIdentity;

        /// <summary>
        /// Then publishes a command.
        /// </summary>
        IAfterPublishBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent> ThenPublish<TCommandAggregate, TCommandAggregateIdentity>(
            Func<IDomainEvent<TAggregate, TAggregateIdentity, TAggregateEvent>, TSaga, ICommand<TCommandAggregate, TCommandAggregateIdentity, IExecutionResult>> commandFactory)
            where TCommandAggregate : IAggregateRoot<TCommandAggregateIdentity>
            where TCommandAggregateIdentity : IIdentity;

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

