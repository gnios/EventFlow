using System.Threading;
using System.Threading.Tasks;
using EventFlow.Queries;
using EventFlow.Examples.Simple.ReadModels;

namespace EventFlow.Examples.Simple.Domain.Model.OrderModel.Queries
{
    public class GetOrderQuery : IQuery<OrderReadModel>
    {
        public GetOrderQuery(OrderId orderId)
        {
            OrderId = orderId;
        }

        public OrderId OrderId { get; }
    }

    public class GetOrderQueryHandler : IQueryHandler<GetOrderQuery, OrderReadModel>
    {
        private readonly IQueryProcessor _queryProcessor;

        public GetOrderQueryHandler(IQueryProcessor queryProcessor)
        {
            _queryProcessor = queryProcessor;
        }

        public async Task<OrderReadModel> ExecuteQueryAsync(
            GetOrderQuery query,
            CancellationToken cancellationToken)
        {
            return await _queryProcessor.ProcessAsync(
                new ReadModelByIdQuery<OrderReadModel>(query.OrderId),
                cancellationToken).ConfigureAwait(false);
        }
    }
}

