using System;
using EventFlow.Aggregates;
using EventFlow.Examples.Simple.Domain.Model.BookingModel.Events;
using EventFlow.Exceptions;

namespace EventFlow.Examples.Simple.Domain.Model.BookingModel
{
    public class BookingAggregate : AggregateRoot<BookingAggregate, BookingId>
    {
        private readonly BookingState _state = new BookingState();

        public BookingAggregate(BookingId id) : base(id)
        {
            Register(_state);
        }

        public string CustomerId => _state.CustomerId;
        public string HotelId => _state.HotelId;
        public DateTime CheckInDate => _state.CheckInDate;
        public DateTime CheckOutDate => _state.CheckOutDate;
        public decimal Amount => _state.Amount;
        public string? PromotionCode => _state.PromotionCode;
        public decimal? DiscountAmount => _state.DiscountAmount;
        public string? RoomNumber => _state.RoomNumber;
        public string? RoomType => _state.RoomType;
        public string? TransactionId => _state.TransactionId;
        public string? EmailAddress => _state.EmailAddress;
        public BookingStatus Status => _state.Status;
        public string? ErrorMessage => _state.ErrorMessage;
        public string? CancellationReason => _state.CancellationReason;

        public void Create(
            string customerId,
            string hotelId,
            DateTime checkInDate,
            DateTime checkOutDate,
            decimal amount)
        {
            if (!IsNew)
                throw DomainError.With("Reserva já foi criada");

            if (string.IsNullOrWhiteSpace(customerId))
                throw DomainError.With("CustomerId não pode ser vazio");

            if (string.IsNullOrWhiteSpace(hotelId))
                throw DomainError.With("HotelId não pode ser vazio");

            if (checkInDate >= checkOutDate)
                throw DomainError.With("Data de check-in deve ser anterior à data de check-out");

            if (amount <= 0)
                throw DomainError.With("Valor deve ser maior que zero");

            Emit(new BookingCreatedEvent(
                customerId,
                hotelId,
                checkInDate,
                checkOutDate,
                amount));
        }

        public void ReserveRoom(string roomNumber, string roomType)
        {
            if (IsNew)
                throw DomainError.With("Reserva não foi criada ainda");

            if (Status != BookingStatus.Created)
                throw DomainError.With($"Não é possível reservar quarto. Status atual: {Status}");

            if (string.IsNullOrWhiteSpace(roomNumber))
                throw DomainError.With("Número do quarto não pode ser vazio");

            if (string.IsNullOrWhiteSpace(roomType))
                throw DomainError.With("Tipo do quarto não pode ser vazio");

            Emit(new RoomReservedEvent(roomNumber, roomType));
        }

        public void MarkRoomReservationFailed(string reason)
        {
            if (IsNew)
                throw DomainError.With("Reserva não foi criada ainda");

            if (Status != BookingStatus.Created)
                throw DomainError.With($"Não é possível falhar reserva de quarto. Status atual: {Status}");

            Emit(new RoomReservationFailedEvent(reason));
        }

        public void ProcessPayment(string transactionId, decimal amount)
        {
            if (IsNew)
                throw DomainError.With("Reserva não foi criada ainda");

            if (Status != BookingStatus.RoomReserved)
                throw DomainError.With($"Não é possível processar pagamento. Status atual: {Status}");

            if (string.IsNullOrWhiteSpace(transactionId))
                throw DomainError.With("TransactionId não pode ser vazio");

            if (amount <= 0)
                throw DomainError.With("Valor do pagamento deve ser maior que zero");

            Emit(new PaymentCompletedEvent(transactionId, amount));
        }

        public void MarkPaymentFailed(string reason)
        {
            if (IsNew)
                throw DomainError.With("Reserva não foi criada ainda");

            if (Status != BookingStatus.RoomReserved)
                throw DomainError.With($"Não é possível falhar pagamento. Status atual: {Status}");

            Emit(new PaymentFailedEvent(reason));
        }

        public void SendConfirmationEmail(string emailAddress)
        {
            if (IsNew)
                throw DomainError.With("Reserva não foi criada ainda");

            if (Status != BookingStatus.PaymentCompleted)
                throw DomainError.With($"Não é possível enviar email de confirmação. Status atual: {Status}");

            if (string.IsNullOrWhiteSpace(emailAddress))
                throw DomainError.With("Endereço de email não pode ser vazio");

            Emit(new ConfirmationEmailSentEvent(emailAddress));
        }

        public void MarkConfirmed(string confirmationNumber)
        {
            if (IsNew)
                throw DomainError.With("Reserva não foi criada ainda");

            if (Status != BookingStatus.Confirmed)
                throw DomainError.With($"Não é possível marcar como confirmado. Status atual: {Status}. Email de confirmação deve ser enviado primeiro.");

            // Este método pode ser usado para lógica adicional se necessário
            // O status já está como Confirmed após o ConfirmationEmailSentEvent
        }

        public void MarkFailed(string reason)
        {
            if (Status == BookingStatus.Confirmed || Status == BookingStatus.Cancelled)
                throw DomainError.With($"Reserva já está finalizada. Status atual: {Status}");

            // Este método pode ser usado para lógica adicional se necessário
            // O status já é atualizado pelos eventos específicos
        }

        public void ReleaseRoom()
        {
            if (IsNew)
                throw DomainError.With("Reserva não foi criada ainda");

            if (Status != BookingStatus.RoomReserved && Status != BookingStatus.PaymentCompleted)
                throw DomainError.With($"Não é possível liberar quarto. Status atual: {Status}");

            // Liberar o quarto - em um cenário real, isso poderia emitir um evento específico
            // Por enquanto, apenas valida o estado
        }
    }
}

