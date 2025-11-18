using System;
using EventFlow.Aggregates;
using EventFlow.EventStores;

namespace EventFlow.Examples.Simple.Domain.Model.TarefaModel.Sagas.Events
{
    [EventVersion("TarefaSagaIniciada", 1)]
    public class TarefaSagaIniciadaEvent : AggregateEvent<TarefaSaga, TarefaSagaId>
    {
        public TarefaSagaIniciadaEvent(TarefaId tarefaId, string titulo, DateTime dataInicio)
        {
            TarefaId = tarefaId;
            Titulo = titulo;
            DataInicio = dataInicio;
        }

        public TarefaId TarefaId { get; }
        public string Titulo { get; }
        public DateTime DataInicio { get; }
    }
}

