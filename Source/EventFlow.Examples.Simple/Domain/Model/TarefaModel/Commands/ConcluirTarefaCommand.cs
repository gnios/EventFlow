using System.Threading;
using System.Threading.Tasks;
using EventFlow.Commands;

namespace EventFlow.Examples.Simple.Domain.Model.TarefaModel.Commands
{
    public class ConcluirTarefaCommand : Command<TarefaAggregate, TarefaId>
    {
        public ConcluirTarefaCommand(TarefaId aggregateId)
            : base(aggregateId)
        {
        }
    }

    public class ConcluirTarefaCommandHandler : CommandHandler<TarefaAggregate, TarefaId, ConcluirTarefaCommand>
    {
        public override Task ExecuteAsync(
            TarefaAggregate aggregate,
            ConcluirTarefaCommand command,
            CancellationToken cancellationToken)
        {
            aggregate.Concluir();
            return Task.CompletedTask;
        }
    }
}
