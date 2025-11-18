using System.Threading;
using System.Threading.Tasks;
using EventFlow.Commands;
using EventFlow.Examples.Simple.Domain.Model.BookingModel;
using Microsoft.Extensions.Logging;

namespace EventFlow.Examples.Simple.Domain.Model.BookingModel.Commands
{
    public class SendConfirmationEmailCommand : Command<BookingAggregate, BookingId>
    {
        public string EmailAddress { get; }
        public string ConfirmationNumber { get; }

        public SendConfirmationEmailCommand(BookingId aggregateId, string emailAddress, string confirmationNumber)
            : base(aggregateId)
        {
            EmailAddress = emailAddress;
            ConfirmationNumber = confirmationNumber;
        }
    }

    public class SendConfirmationEmailCommandHandler : CommandHandler<BookingAggregate, BookingId, SendConfirmationEmailCommand>
    {
        private readonly ILogger<SendConfirmationEmailCommandHandler> _logger;

        public SendConfirmationEmailCommandHandler(ILogger<SendConfirmationEmailCommandHandler> logger)
        {
            _logger = logger;
        }

        public override Task ExecuteAsync(
            BookingAggregate aggregate,
            SendConfirmationEmailCommand command,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "[COMMAND] SendConfirmationEmailCommand - Executando | BookingId: {BookingId} | Email: {Email} | ConfirmationNumber: {ConfirmationNumber}",
                command.AggregateId.Value, command.EmailAddress, command.ConfirmationNumber);

            // Simula envio de email - em produção, isso chamaria um serviço de email
            // Por simplicidade, sempre envia com sucesso
            aggregate.SendConfirmationEmail(command.EmailAddress);

            _logger.LogInformation(
                "[COMMAND] SendConfirmationEmailCommand - Concluído | BookingId: {BookingId} | Email: {Email}",
                command.AggregateId.Value, command.EmailAddress);

            return Task.CompletedTask;
        }
    }
}

