using System;
using EventFlow.Aggregates;
using EventFlow.EventStores;

namespace EventFlow.Examples.Simple.Domain.Model.TarefaModel.Sagas.Events
{
    [EventVersion("TarefaSagaValidacaoSolicitada", 1)]
    public class TarefaSagaValidacaoSolicitadaEvent : AggregateEvent<TarefaSaga, TarefaSagaId>
    {
        public TarefaSagaValidacaoSolicitadaEvent(string motivo, DateTime dataValidacao)
        {
            Motivo = motivo;
            DataValidacao = dataValidacao;
        }

        public string Motivo { get; }
        public DateTime DataValidacao { get; }
    }
}

