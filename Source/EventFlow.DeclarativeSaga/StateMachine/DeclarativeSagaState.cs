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
using EventFlow.DeclarativeSaga.StateMachine.Events;

namespace EventFlow.DeclarativeSaga.StateMachine
{
    /// <summary>
    /// Base class for declarative saga states that automatically handles compensation job ID management.
    /// Developers only need to inherit from this class - no need to create events or implement interfaces manually.
    /// The CompensationJobId is automatically persisted through events when timeouts are configured.
    /// </summary>
    /// <typeparam name="TSaga">The saga type</typeparam>
    /// <typeparam name="TIdentity">The saga identity type</typeparam>
    /// <typeparam name="TState">The state type (CRTP pattern)</typeparam>
    public abstract class DeclarativeSagaState<TSaga, TIdentity, TState> 
        : AggregateState<TSaga, TIdentity, TState>,
          IHasCompensationJobId,
          IApply<CompensationJobIdStoredEvent<TSaga, TIdentity>>,
          IApply<CompensationJobIdClearedEvent<TSaga, TIdentity>>
        where TSaga : IAggregateRoot<TIdentity>
        where TIdentity : Core.IIdentity
        where TState : DeclarativeSagaState<TSaga, TIdentity, TState>
    {
        /// <summary>
        /// Gets or sets the Hangfire job ID for scheduled compensation commands.
        /// This property is automatically managed - developers don't need to handle it manually.
        /// </summary>
        public string? CompensationJobId { get; set; }

        /// <summary>
        /// Automatically applies the CompensationJobIdStoredEvent.
        /// Developers don't need to implement this - it's handled by the base class.
        /// </summary>
        public void Apply(CompensationJobIdStoredEvent<TSaga, TIdentity> aggregateEvent)
        {
            CompensationJobId = aggregateEvent.JobId;
        }

        /// <summary>
        /// Automatically applies the CompensationJobIdClearedEvent.
        /// Developers don't need to implement this - it's handled by the base class.
        /// </summary>
        public void Apply(CompensationJobIdClearedEvent<TSaga, TIdentity> aggregateEvent)
        {
            CompensationJobId = null;
        }
    }
}

