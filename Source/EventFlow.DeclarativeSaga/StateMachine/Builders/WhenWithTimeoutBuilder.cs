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

namespace EventFlow.DeclarativeSaga.StateMachine.Builders
{
    /// <summary>
    /// Builder for configuring actions when an event occurs with timeout.
    /// Allows defining the command to be published and the compensation to be executed if timeout occurs.
    /// This class forces the developer to define compensation after publishing the command.
    /// </summary>
    public class WhenWithTimeoutBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>
        where TSaga : DeclarativeSaga<TSaga, TIdentity, TLocator>
        where TIdentity : ISagaId
        where TLocator : ISagaLocator
        where TAggregate : IAggregateRoot<TAggregateIdentity>
        where TAggregateIdentity : IIdentity
        where TAggregateEvent : IAggregateEvent<TAggregate, TAggregateIdentity>
    {
        private readonly DeclarativeSaga<TSaga, TIdentity, TLocator>.EventHandler _handler;
        private readonly TimeSpan _timeout;

        internal WhenWithTimeoutBuilder(
            DeclarativeSaga<TSaga, TIdentity, TLocator>.EventHandler handler,
            TimeSpan timeout)
        {
            _handler = handler;
            _timeout = timeout;
        }

        /// <summary>
        /// Then publishes a command when the event occurs.
        /// Returns WhenWithTimeoutAfterPublishBuilder which REQUIRES compensation definition.
        /// </summary>
        public WhenWithTimeoutAfterPublishBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent> ThenPublish<TCommandAggregate, TCommandAggregateIdentity>(
            Func<IDomainEvent<TAggregate, TAggregateIdentity, TAggregateEvent>, TSaga, ICommand<TCommandAggregate, TCommandAggregateIdentity, IExecutionResult>> commandFactory)
            where TCommandAggregate : IAggregateRoot<TCommandAggregateIdentity>
            where TCommandAggregateIdentity : IIdentity
        {
            // Adds action to publish the command when the event occurs
            _handler.AddAction((domainEvent, saga, ct) =>
            {
                if (domainEvent is IDomainEvent<TAggregate, TAggregateIdentity, TAggregateEvent> typedEvent)
                {
                    var command = commandFactory(typedEvent, saga);
                    saga.PublishCommand(command);
                }
                return Task.CompletedTask;
            });

            return new WhenWithTimeoutAfterPublishBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>(_handler, _timeout);
        }
    }
}

