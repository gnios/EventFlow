using System;
using System.Threading;
using System.Threading.Tasks;
using EventFlow.Aggregates;
using EventFlow.Examples.Simple.Domain.Model.TarefaModel;
using EventFlow.Examples.Simple.Domain.Model.TarefaModel.Events;
using EventFlow.ReadStores;

namespace EventFlow.Examples.Simple.ReadModels
{
    public class TarefaReadModel : IReadModel,
        IAmReadModelFor<TarefaAggregate, TarefaId, TarefaCriadaEvent>,
        IAmReadModelFor<TarefaAggregate, TarefaId, TarefaAtualizadaEvent>,
        IAmReadModelFor<TarefaAggregate, TarefaId, TarefaConcluidaEvent>
    {
        public string Id { get; private set; } = string.Empty;
        public string Titulo { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;
        public TarefaStatus Status { get; private set; } = TarefaStatus.NaoCriada;
        public DateTime DataCriacao { get; private set; }
        public DateTime? DataAtualizacao { get; private set; }
        public DateTime? DataConclusao { get; private set; }

        public Task ApplyAsync(
            IReadModelContext context,
            IDomainEvent<TarefaAggregate, TarefaId, TarefaCriadaEvent> domainEvent,
            CancellationToken cancellationToken)
        {
            Id = domainEvent.AggregateIdentity.Value;
            Titulo = domainEvent.AggregateEvent.Titulo;
            Descricao = domainEvent.AggregateEvent.Descricao;
            Status = TarefaStatus.Criada;
            DataCriacao = domainEvent.AggregateEvent.DataCriacao;
            return Task.CompletedTask;
        }

        public Task ApplyAsync(
            IReadModelContext context,
            IDomainEvent<TarefaAggregate, TarefaId, TarefaAtualizadaEvent> domainEvent,
            CancellationToken cancellationToken)
        {
            Titulo = domainEvent.AggregateEvent.Titulo;
            Descricao = domainEvent.AggregateEvent.Descricao;
            DataAtualizacao = domainEvent.AggregateEvent.DataAtualizacao;
            return Task.CompletedTask;
        }

        public Task ApplyAsync(
            IReadModelContext context,
            IDomainEvent<TarefaAggregate, TarefaId, TarefaConcluidaEvent> domainEvent,
            CancellationToken cancellationToken)
        {
            Status = TarefaStatus.Concluida;
            DataConclusao = domainEvent.AggregateEvent.DataConclusao;
            return Task.CompletedTask;
        }
    }
}
