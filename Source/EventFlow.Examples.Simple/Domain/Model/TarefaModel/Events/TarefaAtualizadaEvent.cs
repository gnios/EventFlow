using System;
using EventFlow.Aggregates;
using EventFlow.EventStores;

namespace EventFlow.Examples.Simple.Domain.Model.TarefaModel.Events
{
    [EventVersion("TarefaAtualizada", 1)]
    public class TarefaAtualizadaEvent : AggregateEvent<TarefaAggregate, TarefaId>
    {
        public TarefaAtualizadaEvent(string titulo, string descricao, DateTime dataAtualizacao)
        {
            Titulo = titulo;
            Descricao = descricao;
            DataAtualizacao = dataAtualizacao;
        }

        public string Titulo { get; }
        public string Descricao { get; }
        public DateTime DataAtualizacao { get; }
    }
}
