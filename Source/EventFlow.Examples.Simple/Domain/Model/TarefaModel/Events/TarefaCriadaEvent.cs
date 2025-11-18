using System;
using EventFlow.Aggregates;
using EventFlow.EventStores;

namespace EventFlow.Examples.Simple.Domain.Model.TarefaModel.Events
{
    [EventVersion("TarefaCriada", 1)]
    public class TarefaCriadaEvent : AggregateEvent<TarefaAggregate, TarefaId>
    {
        public TarefaCriadaEvent(string titulo, string descricao, DateTime dataCriacao)
        {
            Titulo = titulo;
            Descricao = descricao;
            DataCriacao = dataCriacao;
        }

        public string Titulo { get; }
        public string Descricao { get; }
        public DateTime DataCriacao { get; }
    }
}
