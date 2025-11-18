using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EventFlow.Aggregates;
using EventFlow.Aggregates.ExecutionResults;
using EventFlow.Commands;
using EventFlow.DeclarativeSaga.StateMachine;
using EventFlow.DeclarativeSaga.StateMachine.Builders;
using EventFlow.Examples.Simple.Domain.Model.BookingModel.Commands;
using EventFlow.Examples.Simple.Domain.Model.BookingModel.Events;
using EventFlow.Examples.Simple.Domain.Model.BookingModel.Sagas.Events;
using EventFlow.Sagas;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EventFlow.Examples.Simple.Domain.Model.BookingModel.Sagas
{
    /// <summary>
    /// Comprehensive example saga demonstrating ALL builder capabilities:
    /// - Multiple initial events (3 different ways to start the saga)
    /// - Multiple happy paths
    /// - Multiple failure paths
    /// - Timeouts with compensation
    /// - Scheduled commands
    /// - Chained actions (Then, ThenAsync)
    /// - Multiple actions in sequence
    /// </summary>
    public class BookingSaga : DeclarativeSaga<BookingSaga, BookingSagaId, BookingSagaLocator>,
        ISagaIsStartedBy<BookingAggregate, BookingId, BookingCreatedEvent>,
        ISagaIsStartedBy<BookingAggregate, BookingId, BookingCreatedWithPromotionEvent>,
        ISagaIsStartedBy<BookingAggregate, BookingId, LastMinuteBookingCreatedEvent>,
        ISagaHandles<BookingAggregate, BookingId, RoomReservedEvent>,
        ISagaHandles<BookingAggregate, BookingId, RoomReservationFailedEvent>,
        ISagaHandles<BookingAggregate, BookingId, PaymentCompletedEvent>,
        ISagaHandles<BookingAggregate, BookingId, PaymentFailedEvent>,
        ISagaHandles<BookingAggregate, BookingId, ConfirmationEmailSentEvent>,
        ISagaHandles<BookingAggregate, BookingId, BookingCancelledEvent>
    {
        private readonly ILogger<BookingSaga>? _logger;
        private readonly BookingSagaState _sagaState;

        public BookingSaga(BookingSagaId id, IServiceProvider serviceProvider)
            : base(id, serviceProvider)
        {
            _logger = serviceProvider?.GetService<ILogger<BookingSaga>>();
            _sagaState = RegisterState(new BookingSagaState());

            Define(builder =>
            {
                ConfigureSaga(builder);
            });
        }

        private void ConfigureSaga(DeclarativeSagaBuilder<BookingSaga, BookingSagaId, BookingSagaLocator> builder)
        {
            // ====================================================================
            // MULTIPLE INITIAL EVENTS - 3 different ways to start the saga
            // ====================================================================

            // Path 1: Standard booking creation
            builder.Initially()
                .When<BookingAggregate, BookingId, BookingCreatedEvent>()
                .Then(LogStandardBookingCreated)
                .ThenEmitSagaEvent(CreateSagaStartedEventFromBookingCreated)
                .ThenPublish<BookingAggregate, BookingId>(PublishReserveRoomCommand);

            // Path 2: Booking with promotion
            builder.Initially()
                .When<BookingAggregate, BookingId, BookingCreatedWithPromotionEvent>()
                .ThenAsync(ProcessPromotionAsync)
                .ThenEmitSagaEvent(CreateSagaStartedEventFromPromotion)
                .ThenPublish<BookingAggregate, BookingId>(PublishReserveRoomCommandFromPromotion)
                .ThenSchedule<BookingAggregate, BookingId>(CreateReminderCommandFromPromotion, TimeSpan.FromDays(1));

            // Path 3: Last minute booking (urgent processing)
            builder.Initially()
                .When<BookingAggregate, BookingId, LastMinuteBookingCreatedEvent>()
                .ThenEmitSagaEvent(CreateSagaStartedEventFromLastMinute)
                .ThenPublish<BookingAggregate, BookingId>(PublishReserveRoomCommandFromLastMinute)
                .ThenSchedule<BookingAggregate, BookingId>(CreateReminderCommandFromLastMinute, TimeSpan.FromHours(2));

            builder.When<BookingAggregate, BookingId, RoomReservedEvent>(timeoutMinutes: 15)
                .ThenPublish<BookingAggregate, BookingId>(PublishProcessPaymentCommand)
                .AndCompensateWith<BookingAggregate, BookingId>(CreateReleaseRoomCommand);

            builder.DefinePaths(paths =>
            {
                // Success Path 1: Payment completed → Send confirmation email → Mark as confirmed
                paths.When<BookingAggregate, BookingId, PaymentCompletedEvent>()
                    .ThenEmitSagaEvent(CreatePaymentTimeoutEvent)
                    .ThenPublish<BookingAggregate, BookingId>(PublishSendConfirmationEmailCommand)
                    .ThenSchedule<BookingAggregate, BookingId>(CreateReminderCommandForPaymentCompleted, TimeSpan.FromDays(7));

                // Success Path 2: Confirmation email sent → Mark booking as confirmed → Complete saga
                paths.When<BookingAggregate, BookingId, ConfirmationEmailSentEvent>()
                    .ThenPublish<BookingAggregate, BookingId>(PublishMarkBookingConfirmedCommand)
                    .ThenEmitSagaEvent(CreateSagaCompletedEvent)
                    .AndComplete();

                // ┌─────────────────────────────────────────────────────────┐
                // │ FAILURE PATHS (Alternative Paths)                       │
                // │ These paths lead to booking failure and saga completion│
                // └─────────────────────────────────────────────────────────┘

                // Failure Path 1: Room reservation failed → Mark booking as failed → Complete saga
                paths.When<BookingAggregate, BookingId, RoomReservationFailedEvent>()
                    .Then(LogRoomReservationFailed)
                    .ThenPublish<BookingAggregate, BookingId>(PublishMarkBookingFailedForRoomReservation)
                    .ThenEmitSagaEvent(CreateSagaFailedEventForRoomReservation)
                    .ThenComplete();

                // Failure Path 2: Payment failed → Release room → Refund (if payment was captured) → Mark as failed
                paths.When<BookingAggregate, BookingId, PaymentFailedEvent>()
                    .ThenPublishMany<BookingAggregate, BookingId>(PublishPaymentFailedCommands)
                    .ThenEmitSagaEvent(CreateSagaFailedEventForPayment)
                    .AndComplete();

                // Failure Path 3: Booking cancelled → Release room → Refund payment → Complete saga
                paths.When<BookingAggregate, BookingId, BookingCancelledEvent>()
                    .ThenAsync(ProcessBookingCancellationAsync)
                    .ThenPublishMany<BookingAggregate, BookingId>(PublishBookingCancellationCommands)
                    .ThenEmitSagaEvent(CreateSagaFailedEventForCancellation)
                    .AndComplete();
            });
        }

        // Métodos auxiliares para eventos iniciais
        private void LogStandardBookingCreated(
            IDomainEvent<BookingAggregate, BookingId, BookingCreatedEvent> evt,
            BookingSaga saga)
        {
            saga.LogInfo("Standard booking created", evt.AggregateIdentity);
        }

        private BookingSagaStartedEvent CreateSagaStartedEventFromBookingCreated(
            IDomainEvent<BookingAggregate, BookingId, BookingCreatedEvent> evt,
            BookingSaga saga)
        {
            return new BookingSagaStartedEvent(
                evt.AggregateIdentity.Value,
                evt.AggregateEvent.CustomerId,
                evt.AggregateEvent.HotelId,
                DateTime.UtcNow);
        }

        private ReserveRoomCommand PublishReserveRoomCommand(
            IDomainEvent<BookingAggregate, BookingId, BookingCreatedEvent> evt,
            BookingSaga saga)
        {
            return new ReserveRoomCommand(evt.AggregateIdentity, evt.AggregateEvent.HotelId, evt.AggregateEvent.CheckInDate, evt.AggregateEvent.CheckOutDate);
        }

        private async Task ProcessPromotionAsync(
            IDomainEvent<BookingAggregate, BookingId, BookingCreatedWithPromotionEvent> evt,
            BookingSaga saga,
            CancellationToken ct)
        {
            saga.LogInfo($"Promotion applied: {evt.AggregateEvent.PromotionCode}, Discount: {evt.AggregateEvent.DiscountAmount}", evt.AggregateIdentity);
            await Task.Delay(100, ct); // Simulate async operation
        }

        private BookingSagaStartedEvent CreateSagaStartedEventFromPromotion(
            IDomainEvent<BookingAggregate, BookingId, BookingCreatedWithPromotionEvent> evt,
            BookingSaga saga)
        {
            return new BookingSagaStartedEvent(
                evt.AggregateIdentity.Value,
                evt.AggregateEvent.CustomerId,
                evt.AggregateEvent.HotelId,
                DateTime.UtcNow);
        }

        private ReserveRoomCommand PublishReserveRoomCommandFromPromotion(
            IDomainEvent<BookingAggregate, BookingId, BookingCreatedWithPromotionEvent> evt,
            BookingSaga saga)
        {
            return new ReserveRoomCommand(evt.AggregateIdentity, evt.AggregateEvent.HotelId, evt.AggregateEvent.CheckInDate, evt.AggregateEvent.CheckOutDate);
        }

        private SendReminderCommand CreateReminderCommandFromPromotion(
            IDomainEvent<BookingAggregate, BookingId, BookingCreatedWithPromotionEvent> evt,
            BookingSaga saga)
        {
            return new SendReminderCommand(evt.AggregateIdentity, evt.AggregateEvent.CustomerId, evt.AggregateEvent.CheckInDate);
        }

        private BookingSagaStartedEvent CreateSagaStartedEventFromLastMinute(
            IDomainEvent<BookingAggregate, BookingId, LastMinuteBookingCreatedEvent> evt,
            BookingSaga saga)
        {
            return new BookingSagaStartedEvent(
                evt.AggregateIdentity.Value,
                evt.AggregateEvent.CustomerId,
                evt.AggregateEvent.HotelId,
                DateTime.UtcNow);
        }

        private ReserveRoomCommand PublishReserveRoomCommandFromLastMinute(
            IDomainEvent<BookingAggregate, BookingId, LastMinuteBookingCreatedEvent> evt,
            BookingSaga saga)
        {
            return new ReserveRoomCommand(evt.AggregateIdentity, evt.AggregateEvent.HotelId, evt.AggregateEvent.CheckInDate, evt.AggregateEvent.CheckOutDate);
        }

        private SendReminderCommand CreateReminderCommandFromLastMinute(
            IDomainEvent<BookingAggregate, BookingId, LastMinuteBookingCreatedEvent> evt,
            BookingSaga saga)
        {
            return new SendReminderCommand(evt.AggregateIdentity, evt.AggregateEvent.CustomerId, evt.AggregateEvent.CheckInDate);
        }

        // Métodos auxiliares para eventos de processamento
        private ProcessPaymentCommand PublishProcessPaymentCommand(
            IDomainEvent<BookingAggregate, BookingId, RoomReservedEvent> evt,
            BookingSaga saga)
        {
            saga.LogInfo($"Room {evt.AggregateEvent.RoomNumber} reserved (Type: {evt.AggregateEvent.RoomType})", evt.AggregateIdentity);
            // We need to get the amount from the saga state or event metadata
            // For this example, we'll use a default value
            return new ProcessPaymentCommand(evt.AggregateIdentity, 100.00m, "CreditCard");
        }

        private ReleaseRoomCommand CreateReleaseRoomCommand(
            IDomainEvent<BookingAggregate, BookingId, RoomReservedEvent> evt,
            BookingSaga saga)
        {
            return new ReleaseRoomCommand(evt.AggregateIdentity, evt.AggregateEvent.RoomNumber);
        }

        private BookingSagaPaymentTimeoutEvent CreatePaymentTimeoutEvent(
            IDomainEvent<BookingAggregate, BookingId, PaymentCompletedEvent> evt,
            BookingSaga saga)
        {
            return new BookingSagaPaymentTimeoutEvent(evt.AggregateIdentity.Value, DateTime.UtcNow);
        }

        private SendConfirmationEmailCommand PublishSendConfirmationEmailCommand(
            IDomainEvent<BookingAggregate, BookingId, PaymentCompletedEvent> evt,
            BookingSaga saga)
        {
            return new SendConfirmationEmailCommand(evt.AggregateIdentity, "customer@example.com", $"CONF-{evt.AggregateIdentity.Value}");
        }

        private SendReminderCommand CreateReminderCommandForPaymentCompleted(
            IDomainEvent<BookingAggregate, BookingId, PaymentCompletedEvent> evt,
            BookingSaga saga)
        {
            return new SendReminderCommand(evt.AggregateIdentity, "customer@example.com", DateTime.UtcNow.AddDays(1));
        }

        private MarkBookingConfirmedCommand PublishMarkBookingConfirmedCommand(
            IDomainEvent<BookingAggregate, BookingId, ConfirmationEmailSentEvent> evt,
            BookingSaga saga)
        {
            return new MarkBookingConfirmedCommand(evt.AggregateIdentity, $"CONF-{evt.AggregateIdentity.Value}");
        }

        private BookingSagaCompletedEvent CreateSagaCompletedEvent(
            IDomainEvent<BookingAggregate, BookingId, ConfirmationEmailSentEvent> evt,
            BookingSaga saga)
        {
            return new BookingSagaCompletedEvent(
                evt.AggregateIdentity.Value,
                $"CONF-{evt.AggregateIdentity.Value}",
                DateTime.UtcNow);
        }

        // Métodos auxiliares para caminhos de falha
        private void LogRoomReservationFailed(
            IDomainEvent<BookingAggregate, BookingId, RoomReservationFailedEvent> evt,
            BookingSaga saga)
        {
            saga.LogError($"Room reservation failed: {evt.AggregateEvent.Reason}", evt.AggregateIdentity);
        }

        private MarkBookingFailedCommand PublishMarkBookingFailedForRoomReservation(
            IDomainEvent<BookingAggregate, BookingId, RoomReservationFailedEvent> evt,
            BookingSaga saga)
        {
            return new MarkBookingFailedCommand(evt.AggregateIdentity, evt.AggregateEvent.Reason);
        }

        private BookingSagaFailedEvent CreateSagaFailedEventForRoomReservation(
            IDomainEvent<BookingAggregate, BookingId, RoomReservationFailedEvent> evt,
            BookingSaga saga)
        {
            return new BookingSagaFailedEvent(
                evt.AggregateIdentity.Value,
                evt.AggregateEvent.Reason,
                DateTime.UtcNow);
        }

        private IEnumerable<ICommand<BookingAggregate, BookingId, IExecutionResult>> PublishPaymentFailedCommands(
            IDomainEvent<BookingAggregate, BookingId, PaymentFailedEvent> evt,
            BookingSaga saga)
        {
            saga.LogError($"Payment failed: {evt.AggregateEvent.Reason}", evt.AggregateIdentity);
            return new ICommand<BookingAggregate, BookingId, IExecutionResult>[]
            {
                new ReleaseRoomCommand(evt.AggregateIdentity, "ROOM-001"),
                new MarkBookingFailedCommand(evt.AggregateIdentity, $"Payment failed: {evt.AggregateEvent.Reason}")
            };
        }

        private BookingSagaFailedEvent CreateSagaFailedEventForPayment(
            IDomainEvent<BookingAggregate, BookingId, PaymentFailedEvent> evt,
            BookingSaga saga)
        {
            return new BookingSagaFailedEvent(
                evt.AggregateIdentity.Value,
                $"Payment failed: {evt.AggregateEvent.Reason}",
                DateTime.UtcNow);
        }

        private async Task ProcessBookingCancellationAsync(
            IDomainEvent<BookingAggregate, BookingId, BookingCancelledEvent> evt,
            BookingSaga saga,
            CancellationToken ct)
        {
            saga.LogInfo($"Booking cancelled: {evt.AggregateEvent.Reason}", evt.AggregateIdentity);
            await Task.Delay(100, ct); // Simulate async cancellation processing
        }

        private IEnumerable<ICommand<BookingAggregate, BookingId, IExecutionResult>> PublishBookingCancellationCommands(
            IDomainEvent<BookingAggregate, BookingId, BookingCancelledEvent> evt,
            BookingSaga saga)
        {
            return new ICommand<BookingAggregate, BookingId, IExecutionResult>[]
            {
                new ReleaseRoomCommand(evt.AggregateIdentity, "ROOM-001"),
                new RefundPaymentCommand(evt.AggregateIdentity, "TXN-123", 100.00m)
            };
        }

        private BookingSagaFailedEvent CreateSagaFailedEventForCancellation(
            IDomainEvent<BookingAggregate, BookingId, BookingCancelledEvent> evt,
            BookingSaga saga)
        {
            return new BookingSagaFailedEvent(
                evt.AggregateIdentity.Value,
                $"Cancelled: {evt.AggregateEvent.Reason}",
                DateTime.UtcNow);
        }

        // ====================================================================
        // Event Handlers - All delegate to HandleEventAsync
        // ====================================================================

        public Task HandleAsync(IDomainEvent<BookingAggregate, BookingId, BookingCreatedEvent> domainEvent, ISagaContext sagaContext, CancellationToken cancellationToken)
            => HandleEventAsync(domainEvent, sagaContext, cancellationToken);

        public Task HandleAsync(IDomainEvent<BookingAggregate, BookingId, BookingCreatedWithPromotionEvent> domainEvent, ISagaContext sagaContext, CancellationToken cancellationToken)
            => HandleEventAsync(domainEvent, sagaContext, cancellationToken);

        public Task HandleAsync(IDomainEvent<BookingAggregate, BookingId, LastMinuteBookingCreatedEvent> domainEvent, ISagaContext sagaContext, CancellationToken cancellationToken)
            => HandleEventAsync(domainEvent, sagaContext, cancellationToken);

        public Task HandleAsync(IDomainEvent<BookingAggregate, BookingId, RoomReservedEvent> domainEvent, ISagaContext sagaContext, CancellationToken cancellationToken)
            => HandleEventAsync(domainEvent, sagaContext, cancellationToken);

        public Task HandleAsync(IDomainEvent<BookingAggregate, BookingId, RoomReservationFailedEvent> domainEvent, ISagaContext sagaContext, CancellationToken cancellationToken)
            => HandleEventAsync(domainEvent, sagaContext, cancellationToken);

        public Task HandleAsync(IDomainEvent<BookingAggregate, BookingId, PaymentCompletedEvent> domainEvent, ISagaContext sagaContext, CancellationToken cancellationToken)
            => HandleEventAsync(domainEvent, sagaContext, cancellationToken);

        public Task HandleAsync(IDomainEvent<BookingAggregate, BookingId, PaymentFailedEvent> domainEvent, ISagaContext sagaContext, CancellationToken cancellationToken)
            => HandleEventAsync(domainEvent, sagaContext, cancellationToken);

        public Task HandleAsync(IDomainEvent<BookingAggregate, BookingId, ConfirmationEmailSentEvent> domainEvent, ISagaContext sagaContext, CancellationToken cancellationToken)
            => HandleEventAsync(domainEvent, sagaContext, cancellationToken);

        public Task HandleAsync(IDomainEvent<BookingAggregate, BookingId, BookingCancelledEvent> domainEvent, ISagaContext sagaContext, CancellationToken cancellationToken)
            => HandleEventAsync(domainEvent, sagaContext, cancellationToken);

        // ====================================================================
        // Helper Methods
        // ====================================================================

        private void LogInfo(string message, BookingId bookingId)
        {
            _logger?.LogInformation("[BOOKING SAGA] {Message} | BookingId: {BookingId} | SagaId: {SagaId}",
                message, bookingId.Value, Id.Value);
        }

        private void LogError(string message, BookingId bookingId)
        {
            _logger?.LogError("[BOOKING SAGA] {Message} | BookingId: {BookingId} | SagaId: {SagaId}",
                message, bookingId.Value, Id.Value);
        }
    }
}

