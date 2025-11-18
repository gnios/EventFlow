using System.Threading;
using System.Threading.Tasks;
using EventFlow.Commands;

namespace EventFlow.Examples.Simple.Domain.Model.TarefaModel.Commands
{
    public class CriarTarefaCommand : Command<TarefaAggregate, TarefaId>
    {
        public CriarTarefaCommand(
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

    public class CriarTarefaCommandHandler : CommandHandler<TarefaAggregate, TarefaId, CriarTarefaCommand>
    {
        public override Task ExecuteAsync(
            TarefaAggregate aggregate,
            CriarTarefaCommand command,
            CancellationToken cancellationToken)
        {
            aggregate.Criar(command.Titulo, command.Descricao);
            return Task.CompletedTask;
        }
    }
}
