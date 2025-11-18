using System;
using EventFlow.Aggregates;
using EventFlow.EventStores;

namespace EventFlow.Examples.Simple.Domain.Model.TarefaModel.Events
{
    [EventVersion("TarefaConcluida", 1)]
    public class TarefaConcluidaEvent : AggregateEvent<TarefaAggregate, TarefaId>
    {
        public TarefaConcluidaEvent(DateTime dataConclusao)
        {
            DataConclusao = dataConclusao;
        }

        public DateTime DataConclusao { get; }
    }
}
