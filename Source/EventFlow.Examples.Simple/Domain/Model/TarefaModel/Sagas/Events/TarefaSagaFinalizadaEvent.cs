using System;
using EventFlow.Aggregates;
using EventFlow.EventStores;

namespace EventFlow.Examples.Simple.Domain.Model.TarefaModel.Sagas.Events
{
    [EventVersion("TarefaSagaFinalizada", 1)]
    public class TarefaSagaFinalizadaEvent : AggregateEvent<TarefaSaga, TarefaSagaId>
    {
        public TarefaSagaFinalizadaEvent(
            DateTime dataFinalizacao, 
            int totalAtualizacoes,
            bool revisaoSolicitada,
            bool validacaoRealizada)
        {
            DataFinalizacao = dataFinalizacao;
            TotalAtualizacoes = totalAtualizacoes;
            RevisaoSolicitada = revisaoSolicitada;
            ValidacaoRealizada = validacaoRealizada;
        }

        public DateTime DataFinalizacao { get; }
        public int TotalAtualizacoes { get; }
        public bool RevisaoSolicitada { get; }
        public bool ValidacaoRealizada { get; }
    }
}

