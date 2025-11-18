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
using EventFlow.EventStores;

namespace EventFlow.DeclarativeSaga.StateMachine.Events
{
    /// <summary>
    /// Base event class for clearing compensation job ID from saga state.
    /// Each saga should create its own specific version of this event.
    /// This event is emitted when the expected event arrives before timeout,
    /// causing the compensation job to be cancelled and removed from the state.
    /// 
    /// Note: This event does not specify a name in EventVersion to allow EventFlow to use the full generic type name,
    /// ensuring each saga has a unique event name (e.g., CompensationJobIdClearedEvent`2[OrderDeclarativeSaga,OrderSagaId]).
    /// </summary>
    /// <typeparam name="TSaga">The saga type</typeparam>
    /// <typeparam name="TIdentity">The saga identity type</typeparam>
    [EventVersion(1)]
    public class CompensationJobIdClearedEvent<TSaga, TIdentity> : AggregateEvent<TSaga, TIdentity>
        where TSaga : IAggregateRoot<TIdentity>
        where TIdentity : Core.IIdentity
    {
    }
}

