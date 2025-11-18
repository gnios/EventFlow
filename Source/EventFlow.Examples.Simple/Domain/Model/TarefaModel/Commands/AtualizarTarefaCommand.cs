using System.Threading;
using System.Threading.Tasks;
using EventFlow.Commands;

namespace EventFlow.Examples.Simple.Domain.Model.TarefaModel.Commands
{
    public class AtualizarTarefaCommand : Command<TarefaAggregate, TarefaId>
    {
        public AtualizarTarefaCommand(
            TarefaId aggregateId,
            string titulo,
            string descricao)
            : base(aggregateId)
        {
            Titulo = titulo;
            Descricao = descricao;
        }

        public string Titulo { get; }
        public string Descricao { get; }
    }

    public class AtualizarTarefaCommandHandler : CommandHandler<TarefaAggregate, TarefaId, AtualizarTarefaCommand>
    {
        public override Task ExecuteAsync(
            TarefaAggregate aggregate,
            AtualizarTarefaCommand command,
            CancellationToken cancellationToken)
        {
            aggregate.Atualizar(command.Titulo, command.Descricao);
            return Task.CompletedTask;
        }
    }
}
