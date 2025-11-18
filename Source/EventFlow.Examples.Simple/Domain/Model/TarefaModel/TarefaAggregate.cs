using System;
using EventFlow.Aggregates;
using EventFlow.Examples.Simple.Domain.Model.TarefaModel.Events;
using EventFlow.Exceptions;

namespace EventFlow.Examples.Simple.Domain.Model.TarefaModel
{
    public class TarefaAggregate : AggregateRoot<TarefaAggregate, TarefaId>
    {
        private readonly TarefaState _state = new TarefaState();

        public TarefaAggregate(TarefaId id) : base(id)
        {
            Register(_state);
        }

        public string Titulo => _state.Titulo;
        public string Descricao => _state.Descricao;
        public TarefaStatus Status => _state.Status;
        public DateTime DataCriacao => _state.DataCriacao;

        public void Criar(string titulo, string descricao)
        {
            if (!IsNew)
                throw DomainError.With("Tarefa já foi criada");

            if (string.IsNullOrWhiteSpace(titulo))
                throw DomainError.With("Título não pode ser vazio");

            if (string.IsNullOrWhiteSpace(descricao))
                throw DomainError.With("Descrição não pode ser vazia");

            Emit(new TarefaCriadaEvent(titulo, descricao, DateTime.UtcNow));
        }

        public void Atualizar(string titulo, string descricao)
        {
            if (IsNew)
                throw DomainError.With("Tarefa não foi criada ainda");

            if (Status == TarefaStatus.Concluida)
                throw DomainError.With("Não é possível atualizar uma tarefa concluída");

            if (string.IsNullOrWhiteSpace(titulo))
                throw DomainError.With("Título não pode ser vazio");

            if (string.IsNullOrWhiteSpace(descricao))
                throw DomainError.With("Descrição não pode ser vazia");

            Emit(new TarefaAtualizadaEvent(titulo, descricao, DateTime.UtcNow));
        }

        public void Concluir()
        {
            if (IsNew)
                throw DomainError.With("Tarefa não foi criada ainda");

            if (Status == TarefaStatus.Concluida)
                throw DomainError.With("Tarefa já está concluída");

            Emit(new TarefaConcluidaEvent(DateTime.UtcNow));
        }
    }
}
