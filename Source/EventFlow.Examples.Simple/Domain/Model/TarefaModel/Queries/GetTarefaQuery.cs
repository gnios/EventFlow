using System.Threading;
using System.Threading.Tasks;
using EventFlow.Queries;
using EventFlow.Examples.Simple.ReadModels;

namespace EventFlow.Examples.Simple.Domain.Model.TarefaModel.Queries
{
    public class GetTarefaQuery : IQuery<TarefaReadModel>
    {
        public GetTarefaQuery(TarefaId tarefaId)
        {
            TarefaId = tarefaId;
        }

        public TarefaId TarefaId { get; }
    }

    public class GetTarefaQueryHandler : IQueryHandler<GetTarefaQuery, TarefaReadModel>
    {
        private readonly IQueryProcessor _queryProcessor;

        public GetTarefaQueryHandler(IQueryProcessor queryProcessor)
        {
            _queryProcessor = queryProcessor;
        }

        public async Task<TarefaReadModel> ExecuteQueryAsync(
            GetTarefaQuery query,
            CancellationToken cancellationToken)
        {
            return await _queryProcessor.ProcessAsync(
                new ReadModelByIdQuery<TarefaReadModel>(query.TarefaId),
                cancellationToken).ConfigureAwait(false);
        }
    }
}
