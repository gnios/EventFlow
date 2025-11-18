using System.Threading;
using System.Threading.Tasks;
using EventFlow.Aggregates;
using EventFlow.Sagas;

namespace EventFlow.Examples.Simple.Domain.Model.TarefaModel.Sagas
{
    public class TarefaSagaLocator : ISagaLocator
    {
        public Task<ISagaId> LocateSagaAsync(IDomainEvent domainEvent, CancellationToken cancellationToken)
        {
            // A saga é identificada pelo ID da tarefa
            var tarefaId = domainEvent.GetIdentity();
            var sagaId = new TarefaSagaId($"tarefa-saga-{tarefaId.Value}");
            return Task.FromResult<ISagaId>(sagaId);
        }
    }
}

