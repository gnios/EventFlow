using System;
using EventFlow.Aggregates;
using EventFlow.EventStores;

namespace EventFlow.Examples.Simple.Domain.Model.TarefaModel.Sagas.Events
{
    [EventVersion("TarefaSagaRevisaoSolicitada", 1)]
    public class TarefaSagaRevisaoSolicitadaEvent : AggregateEvent<TarefaSaga, TarefaSagaId>
    {
        public TarefaSagaRevisaoSolicitadaEvent(int numeroAtualizacoes, string motivo, DateTime dataRevisao)
        {
            NumeroAtualizacoes = numeroAtualizacoes;
            Motivo = motivo;
            DataRevisao = dataRevisao;
        }

        public int NumeroAtualizacoes { get; }
        public string Motivo { get; }
        public DateTime DataRevisao { get; }
    }
}

