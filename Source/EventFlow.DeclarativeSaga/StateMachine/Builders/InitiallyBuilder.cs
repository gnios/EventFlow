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
using EventFlow.Core;
using EventFlow.Sagas;

namespace EventFlow.DeclarativeSaga.StateMachine.Builders
{
    /// <summary>
    /// Builder for configuring initial event handlers.
    /// Use this builder to define what happens when the saga is started by an event.
    /// </summary>
    public class InitiallyBuilder<TSaga, TIdentity, TLocator>
        where TSaga : DeclarativeSaga<TSaga, TIdentity, TLocator>
        where TIdentity : ISagaId
        where TLocator : ISagaLocator
    {
        private readonly Dictionary<Type, DeclarativeSaga<TSaga, TIdentity, TLocator>.EventHandler> _eventHandlers;

        internal InitiallyBuilder(Dictionary<Type, DeclarativeSaga<TSaga, TIdentity, TLocator>.EventHandler> eventHandlers)
        {
            _eventHandlers = eventHandlers;
        }

        /// <summary>
        /// Configures a handler for a specific event that starts the saga.
        /// This method should be followed by actions like ThenEmitSagaEvent, ThenPublish, ThenSchedule, etc.
        /// </summary>
        /// <example>
        /// builder.Initially()
        ///     .When&lt;OrderAggregate, OrderId, OrderCreatedEvent&gt;()
        ///     .ThenEmitSagaEvent((evt, saga) => new OrderSagaStartedEvent(...))
        ///     .ThenPublish&lt;OrderAggregate, OrderId&gt;((evt, saga) => new ReserveStockCommand(...));
        /// </example>
        public WhenBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent> When<TAggregate, TAggregateIdentity, TAggregateEvent>()
            where TAggregate : IAggregateRoot<TAggregateIdentity>
            where TAggregateIdentity : IIdentity
            where TAggregateEvent : IAggregateEvent<TAggregate, TAggregateIdentity>
        {
            var eventType = typeof(TAggregateEvent);
            if (!_eventHandlers.TryGetValue(eventType, out var handler))
            {
                handler = new DeclarativeSaga<TSaga, TIdentity, TLocator>.EventHandler();
                _eventHandlers[eventType] = handler;
            }

            return new WhenBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>(handler);
        }
    }
}

