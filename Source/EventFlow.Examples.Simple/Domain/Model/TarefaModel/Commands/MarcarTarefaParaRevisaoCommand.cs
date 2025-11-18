using System.Threading;
using System.Threading.Tasks;
using EventFlow.Commands;

namespace EventFlow.Examples.Simple.Domain.Model.TarefaModel.Commands
{
    /// <summary>
    /// Comando para marcar uma tarefa como requerendo revisão.
    /// Este comando pode ser publicado pela saga quando detecta problemas.
    /// </summary>
    public class MarcarTarefaParaRevisaoCommand : Command<TarefaAggregate, TarefaId>
    {
        public MarcarTarefaParaRevisaoCommand(
            TarefaId aggregateId,
            string motivo)
            : base(aggregateId)
        {
            Motivo = motivo;
        }

        public string Motivo { get; }
    }

    public class MarcarTarefaParaRevisaoCommandHandler : CommandHandler<TarefaAggregate, TarefaId, MarcarTarefaParaRevisaoCommand>
    {
        public override Task ExecuteAsync(
            TarefaAggregate aggregate,
            MarcarTarefaParaRevisaoCommand command,
            CancellationToken cancellationToken)
        {
            // Este comando seria processado pelo agregado se tivéssemos um método correspondente
            // Por enquanto, apenas demonstra como a saga pode publicar comandos
            // Em um cenário real, o agregado teria um método MarcarParaRevisao(motivo)
            return Task.CompletedTask;
        }
    }
}

