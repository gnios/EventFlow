using System.Reflection;
using EventFlow.DeclarativeSaga.StateMachine.Events;
using EventFlow.Extensions;
using EventFlow.Examples.Simple.Domain.Model.BookingModel.Commands;
using EventFlow.Examples.Simple.Domain.Model.BookingModel.Events;
using EventFlow.Examples.Simple.Domain.Model.BookingModel.Sagas;
using EventFlow.Examples.Simple.Domain.Model.BookingModel.Sagas.Events;
using EventFlow.Examples.Simple.Domain.Model.OrderModel;
using EventFlow.Examples.Simple.Domain.Model.OrderModel.Commands;
using EventFlow.Examples.Simple.Domain.Model.OrderModel.Events;
using EventFlow.Examples.Simple.Domain.Model.OrderModel.Queries;
using EventFlow.Examples.Simple.Domain.Model.OrderModel.Sagas;
using EventFlow.Examples.Simple.Domain.Model.OrderModel.Sagas.Events;
using EventFlow.Examples.Simple.ReadModels;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;

namespace EventFlow.Examples.Simple.Configuration
{
    public static class EventFlowConfiguration
    {
        public static Assembly Assembly { get; } = typeof(EventFlowConfiguration).Assembly;

        public static IServiceCollection AddEventFlowConfiguration(this IServiceCollection services)
        {
            return services.AddEventFlow(o => o
                .AddEvents(
                    // Order Events
                    typeof(OrderCreatedEvent),
                    typeof(OrderStockReservedEvent),
                    typeof(OrderStockReservationFailedEvent),
                    typeof(OrderPaymentCompletedEvent),
                    typeof(OrderPaymentFailedEvent),
                    typeof(OrderCompletedEvent),
                    typeof(OrderFailedEvent),
                    typeof(OrderDeclarativeSagaStartedEvent),
                    typeof(OrderDeclarativeSagaCompletedEvent),
                    typeof(OrderDeclarativeSagaFailedEvent),
                    // Booking Events
                    typeof(BookingCreatedEvent),
                    typeof(BookingCreatedWithPromotionEvent),
                    typeof(LastMinuteBookingCreatedEvent),
                    typeof(RoomReservedEvent),
                    typeof(RoomReservationFailedEvent),
                    typeof(PaymentCompletedEvent),
                    typeof(PaymentFailedEvent),
                    typeof(ConfirmationEmailSentEvent),
                    typeof(BookingCancelledEvent),
                    // Booking Saga Events
                    typeof(BookingSagaStartedEvent),
                    typeof(BookingSagaCompletedEvent),
                    typeof(BookingSagaFailedEvent),
                    typeof(BookingSagaPaymentTimeoutEvent),
                    // Compensation Events for OrderDeclarativeSaga
                    typeof(CompensationJobIdStoredEvent<,>).MakeGenericType(typeof(OrderDeclarativeSaga), typeof(OrderSagaId)),
                    typeof(CompensationJobIdClearedEvent<,>).MakeGenericType(typeof(OrderDeclarativeSaga), typeof(OrderSagaId)),
                    // Compensation Events for BookingSaga
                    typeof(CompensationJobIdStoredEvent<,>).MakeGenericType(typeof(BookingSaga), typeof(BookingSagaId)),
                    typeof(CompensationJobIdClearedEvent<,>).MakeGenericType(typeof(BookingSaga), typeof(BookingSagaId)))
                .AddCommands(
                    // Order Commands
                    typeof(CreateOrderCommand),
                    typeof(ReserveStockCommand),
                    typeof(CompletePaymentCommand),
                    typeof(StockRollbackCommand),
                    typeof(MarkOrderCompletedCommand),
                    typeof(MarkOrderFailedCommand),
                    // Booking Commands
                    typeof(CreateBookingCommand),
                    typeof(ReserveRoomCommand),
                    typeof(ProcessPaymentCommand),
                    typeof(RefundPaymentCommand),
                    typeof(ReleaseRoomCommand),
                    typeof(SendConfirmationEmailCommand),
                    typeof(SendReminderCommand),
                    typeof(MarkBookingConfirmedCommand),
                    typeof(MarkBookingFailedCommand))
                .AddCommandHandlers(
                    // Order Command Handlers
                    typeof(CreateOrderCommandHandler),
                    typeof(ReserveStockCommandHandler),
                    typeof(CompletePaymentCommandHandler),
                    typeof(StockRollbackCommandHandler),
                    typeof(MarkOrderCompletedCommandHandler),
                    typeof(MarkOrderFailedCommandHandler),
                    // Booking Command Handlers
                    typeof(CreateBookingCommandHandler),
                    typeof(ReserveRoomCommandHandler),
                    typeof(ProcessPaymentCommandHandler),
                    typeof(ReleaseRoomCommandHandler),
                    typeof(SendConfirmationEmailCommandHandler),
                    typeof(SendReminderCommandHandler),
                    typeof(MarkBookingConfirmedCommandHandler),
                    typeof(MarkBookingFailedCommandHandler))
                .AddQueryHandlers(typeof(GetOrderQueryHandler))
                // Sagas usando API declarativa
                .AddSagas(typeof(OrderDeclarativeSaga), typeof(BookingSaga))
                .AddSagaLocators(typeof(OrderSagaLocator), typeof(BookingSagaLocator))
                .UseInMemoryReadStoreFor<OrderReadModel>()
                // Configurar JSON serializer para ignorar ISourceId nos comandos
                // Isso resolve o problema de deserialização quando comandos são serializados pelo PublishCommandJob
                .ConfigureJson(jsonOptions => jsonOptions.Configure(settings =>
                {
                    settings.ContractResolver = new IgnoreSourceIdContractResolver();
                })));
        }
    }
}
