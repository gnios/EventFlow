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
    /// Main builder for configuring a declarative saga.
    /// This class provides a fluent API inspired by frameworks like MassTransit and WorkflowCore,
    /// allowing to clearly define each step and possibility of the saga.
    /// 
    /// The structure has been organized into separate classes for better maintainability:
    /// - Interfaces: define contracts that ensure the correct path (Interface Segregation)
    /// - Builders: implement the logic for each builder stage
    /// </summary>
    /// <example>
    /// Define(builder => {
    ///     // Initial event - starts the saga
    ///     builder.Initially()
    ///         .When&lt;OrderAggregate, OrderId, OrderCreatedEvent&gt;()
    ///         .ThenEmitSagaEvent((evt, saga) => new OrderSagaStartedEvent(...))
    ///         .ThenPublish&lt;OrderAggregate, OrderId&gt;((evt, saga) => new ReserveStockCommand(...));
    ///     
    ///     // Events during execution - each represents a possible path
    ///     builder.When&lt;OrderAggregate, OrderId, OrderStockReservedEvent&gt;()
    ///         .ThenPublish&lt;OrderAggregate, OrderId&gt;((evt, saga) => new CompletePaymentCommand(...));
    ///     
    ///     builder.When&lt;OrderAggregate, OrderId, OrderStockReservationFailedEvent&gt;()
    ///         .ThenPublish&lt;OrderAggregate, OrderId&gt;((evt, saga) => new MarkOrderFailedCommand(...))
    ///         .ThenComplete();
    /// });
    /// </example>
    public class DeclarativeSagaBuilder<TSaga, TIdentity, TLocator>
        where TSaga : DeclarativeSaga<TSaga, TIdentity, TLocator>
        where TIdentity : ISagaId
        where TLocator : ISagaLocator
    {
        private readonly Dictionary<Type, DeclarativeSaga<TSaga, TIdentity, TLocator>.EventHandler> _eventHandlers;

        internal DeclarativeSagaBuilder(Dictionary<Type, DeclarativeSaga<TSaga, TIdentity, TLocator>.EventHandler> eventHandlers)
        {
            _eventHandlers = eventHandlers;
        }

        /// <summary>
        /// Defines handlers for initial events (that start the saga).
        /// Use this method to configure what happens when the saga is started.
        /// </summary>
        /// <example>
        /// builder.Initially()
        ///     .When&lt;OrderAggregate, OrderId, OrderCreatedEvent&gt;()
        ///     .ThenEmitSagaEvent((evt, saga) => new OrderSagaStartedEvent(...))
        ///     .ThenPublish&lt;OrderAggregate, OrderId&gt;((evt, saga) => new ReserveStockCommand(...));
        /// </example>
        public InitiallyBuilder<TSaga, TIdentity, TLocator> Initially()
        {
            return new InitiallyBuilder<TSaga, TIdentity, TLocator>(_eventHandlers);
        }

        /// <summary>
        /// When an event occurs during saga execution.
        /// IMPORTANT: All paths defined with When() belong to the SAME saga.
        /// Each call to When() represents a possible path (success, failure, etc.) within the same saga.
        /// Use this method for each event that can occur after the saga has been started.
        /// </summary>
        /// <example>
        /// // All these paths are part of the SAME saga:
        /// 
        /// // Path 1: Success - stock reserved
        /// builder.When&lt;OrderAggregate, OrderId, OrderStockReservedEvent&gt;()
        ///     .ThenPublish&lt;OrderAggregate, OrderId&gt;((evt, saga) => new CompletePaymentCommand(...));
        /// 
        /// // Path 2: Failure - stock reservation failed
        /// builder.When&lt;OrderAggregate, OrderId, OrderStockReservationFailedEvent&gt;()
        ///     .ThenPublish&lt;OrderAggregate, OrderId&gt;((evt, saga) => new MarkOrderFailedCommand(...))
        ///     .ThenComplete();
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

        /// <summary>
        /// When an event occurs during saga execution with timeout configured.
        /// If the next event does not arrive within the specified time, the compensation action will be executed.
        /// This overload forces the developer to define compensation through the returned builder.
        /// </summary>
        /// <param name="timeoutMinutes">Time in minutes to wait for the next event before executing compensation</param>
        /// <example>
        /// builder.When&lt;OrderAggregate, OrderId, OrderStockReservedEvent&gt;(timeoutMinutes: 10)
        ///     .ThenPublish&lt;OrderAggregate, OrderId&gt;((evt, saga) => new CompletePaymentCommand(evt.AggregateIdentity))
        ///     .AndCompensateWith&lt;OrderAggregate, OrderId&gt;((evt, saga) => new StockRollbackCommand(evt.AggregateIdentity));
        /// </example>
        public WhenWithTimeoutBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent> When<TAggregate, TAggregateIdentity, TAggregateEvent>(int timeoutMinutes)
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

            return new WhenWithTimeoutBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>(handler, TimeSpan.FromMinutes(timeoutMinutes));
        }

        /// <summary>
        /// Groups the definition of all possible paths of the saga.
        /// This method is optional and serves only to improve readability,
        /// making it clear that all paths belong to the same saga.
        /// </summary>
        /// <param name="configurePaths">Action that configures all possible paths of the saga</param>
        /// <example>
        /// builder.Initially()
        ///     .When&lt;OrderAggregate, OrderId, OrderCreatedEvent&gt;()
        ///     .ThenPublish&lt;OrderAggregate, OrderId&gt;((evt, saga) => new ReserveStockCommand(...));
        /// 
        /// // Groups all possible paths of the same saga
        /// builder.DefinePaths(paths => {
        ///     // Success path
        ///     paths.When&lt;OrderAggregate, OrderId, OrderStockReservedEvent&gt;()
        ///         .ThenPublish&lt;OrderAggregate, OrderId&gt;((evt, saga) => new CompletePaymentCommand(...));
        ///     
        ///     // Failure path
        ///     paths.When&lt;OrderAggregate, OrderId, OrderStockReservationFailedEvent&gt;()
        ///         .ThenPublish&lt;OrderAggregate, OrderId&gt;((evt, saga) => new MarkOrderFailedCommand(...))
        ///         .ThenComplete();
        /// });
        /// </example>
        public void DefinePaths(Action<DeclarativeSagaBuilder<TSaga, TIdentity, TLocator>> configurePaths)
        {
            configurePaths(this);
        }
    }
}
