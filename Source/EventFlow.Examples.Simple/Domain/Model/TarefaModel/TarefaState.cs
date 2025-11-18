using System;
using EventFlow.Aggregates;
using EventFlow.Examples.Simple.Domain.Model.TarefaModel.Events;

namespace EventFlow.Examples.Simple.Domain.Model.TarefaModel
{
    public class TarefaState : AggregateState<TarefaAggregate, TarefaId, TarefaState>,
        IApply<TarefaCriadaEvent>,
        IApply<TarefaAtualizadaEvent>,
        IApply<TarefaConcluidaEvent>
    {
        public string Titulo { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;
        public TarefaStatus Status { get; private set; } = TarefaStatus.NaoCriada;
        public DateTime DataCriacao { get; private set; }
        public DateTime? DataAtualizacao { get; private set; }
        public DateTime? DataConclusao { get; private set; }

        public void Apply(TarefaCriadaEvent aggregateEvent)
        {
            Titulo = aggregateEvent.Titulo;
            Descricao = aggregateEvent.Descricao;
            Status = TarefaStatus.Criada;
            DataCriacao = aggregateEvent.DataCriacao;
        }

        public void Apply(TarefaAtualizadaEvent aggregateEvent)
        {
            Titulo = aggregateEvent.Titulo;
            Descricao = aggregateEvent.Descricao;
            DataAtualizacao = aggregateEvent.DataAtualizacao;
        }

        public void Apply(TarefaConcluidaEvent aggregateEvent)
        {
            Status = TarefaStatus.Concluida;
            DataConclusao = aggregateEvent.DataConclusao;
        }
    }

    public enum TarefaStatus
    {
        NaoCriada,
        Criada,
        Concluida
    }
}
