using System;
using System.Threading;
using System.Threading.Tasks;
using EventFlow.Aggregates;
using EventFlow.Examples.Simple.Domain.Model.TarefaModel.Commands;
using EventFlow.Examples.Simple.Domain.Model.TarefaModel.Events;
using EventFlow.Examples.Simple.Domain.Model.TarefaModel.Sagas.Events;
using EventFlow.Exceptions;
using EventFlow.Sagas;
using EventFlow.Sagas.AggregateSagas;

namespace EventFlow.Examples.Simple.Domain.Model.TarefaModel.Sagas
{
    /// <summary>
    /// Saga que coordena o workflow de validação e revisão de tarefas.
    /// 
    /// Workflow:
    /// 1. Tarefa criada → Saga valida qualidade inicial
    /// 2. Tarefa atualizada → Saga monitora número de atualizações
    /// 3. Muitas atualizações → Saga solicita revisão
    /// 4. Tarefa concluída → Saga finaliza processo
    /// </summary>
    public class TarefaSaga : AggregateSaga<TarefaSaga, TarefaSagaId, TarefaSagaLocator>,
        ISagaIsStartedBy<TarefaAggregate, TarefaId, TarefaCriadaEvent>,
        ISagaHandles<TarefaAggregate, TarefaId, TarefaAtualizadaEvent>,
        ISagaHandles<TarefaAggregate, TarefaId, TarefaConcluidaEvent>,
        IEmit<TarefaSagaIniciadaEvent>,
        IEmit<TarefaSagaValidacaoSolicitadaEvent>,
        IEmit<TarefaSagaRevisaoSolicitadaEvent>,
        IEmit<TarefaSagaFinalizadaEvent>
    {
        private TarefaId? _tarefaId;
        private string _titulo = string.Empty;
        private string _descricao = string.Empty;
        private int _numeroAtualizacoes;
        private DateTime _dataInicio;
        private bool _revisaoSolicitada;
        private bool _validacaoRealizada;

        public TarefaSaga(TarefaSagaId id) : base(id)
        {
        }

        public TarefaId? TarefaId => _tarefaId;
        public string Titulo => _titulo;
        public int NumeroAtualizacoes => _numeroAtualizacoes;
        public DateTime DataInicio => _dataInicio;
        public bool RevisaoSolicitada => _revisaoSolicitada;
        public bool ValidacaoRealizada => _validacaoRealizada;

        /// <summary>
        /// Inicia a saga quando uma tarefa é criada e valida a qualidade inicial
        /// </summary>
        public Task HandleAsync(
            IDomainEvent<TarefaAggregate, TarefaId, TarefaCriadaEvent> domainEvent,
            ISagaContext sagaContext,
            CancellationToken cancellationToken)
        {
            if (State != SagaState.New)
                throw DomainError.With("Saga deve estar no estado New");

            var tarefaId = domainEvent.AggregateIdentity;
            var titulo = domainEvent.AggregateEvent.Titulo;
            var descricao = domainEvent.AggregateEvent.Descricao;

            Emit(new TarefaSagaIniciadaEvent(
                tarefaId,
                titulo,
                DateTime.UtcNow));

            // Regra de negócio: Se o título for muito curto (< 10 caracteres),
            // a saga publica um comando para solicitar mais detalhes
            if (titulo.Length < 10)
            {
                Emit(new TarefaSagaValidacaoSolicitadaEvent(
                    "Título muito curto. Mínimo de 10 caracteres recomendado.",
                    DateTime.UtcNow));

                // Publica comando para marcar tarefa para revisão
                Publish(new MarcarTarefaParaRevisaoCommand(
                    tarefaId,
                    $"Título muito curto ({titulo.Length} caracteres). Mínimo recomendado: 10 caracteres."));
            }
            else if (descricao.Length < 20)
            {
                Emit(new TarefaSagaValidacaoSolicitadaEvent(
                    "Descrição muito curta. Mínimo de 20 caracteres recomendado.",
                    DateTime.UtcNow));

                Publish(new MarcarTarefaParaRevisaoCommand(
                    tarefaId,
                    $"Descrição muito curta ({descricao.Length} caracteres). Mínimo recomendado: 20 caracteres."));
            }
            else
            {
                _validacaoRealizada = true;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Processa atualizações da tarefa e monitora padrões
        /// </summary>
        public Task HandleAsync(
            IDomainEvent<TarefaAggregate, TarefaId, TarefaAtualizadaEvent> domainEvent,
            ISagaContext sagaContext,
            CancellationToken cancellationToken)
        {
            if (State != SagaState.Running)
                throw DomainError.With("Saga deve estar no estado Running");

            _numeroAtualizacoes++;

            // Regra de negócio: Se a tarefa foi atualizada mais de 3 vezes,
            // a saga solicita uma revisão para garantir qualidade
            if (_numeroAtualizacoes > 3 && !_revisaoSolicitada)
            {
                var tarefaId = domainEvent.AggregateIdentity;
                
                Emit(new TarefaSagaRevisaoSolicitadaEvent(
                    _numeroAtualizacoes,
                    "Muitas atualizações detectadas. Revisão recomendada para garantir qualidade.",
                    DateTime.UtcNow));

                // Publica comando para marcar tarefa para revisão
                Publish(new MarcarTarefaParaRevisaoCommand(
                    tarefaId,
                    $"Tarefa atualizada {_numeroAtualizacoes} vezes. Revisão recomendada para garantir qualidade e consistência."));

                _revisaoSolicitada = true;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Finaliza a saga quando a tarefa é concluída
        /// </summary>
        public Task HandleAsync(
            IDomainEvent<TarefaAggregate, TarefaId, TarefaConcluidaEvent> domainEvent,
            ISagaContext sagaContext,
            CancellationToken cancellationToken)
        {
            if (State != SagaState.Running)
                throw DomainError.With("Saga deve estar no estado Running");

            Emit(new TarefaSagaFinalizadaEvent(
                DateTime.UtcNow,
                _numeroAtualizacoes,
                _revisaoSolicitada,
                _validacaoRealizada));

            // Em um cenário real, aqui a saga poderia publicar comandos para:
            // - Enviar notificações
            // - Atualizar estatísticas
            // - Registrar métricas
            // Por exemplo: Publish(new RegistrarMetricaTarefaConcluidaCommand(...));

            return Task.CompletedTask;
        }

        public void Apply(TarefaSagaIniciadaEvent aggregateEvent)
        {
            _tarefaId = aggregateEvent.TarefaId;
            _titulo = aggregateEvent.Titulo;
            _dataInicio = aggregateEvent.DataInicio;
            _numeroAtualizacoes = 0;
            _revisaoSolicitada = false;
            _validacaoRealizada = false;
        }

        public void Apply(TarefaSagaValidacaoSolicitadaEvent aggregateEvent)
        {
            _validacaoRealizada = false;
        }

        public void Apply(TarefaSagaRevisaoSolicitadaEvent aggregateEvent)
        {
            _numeroAtualizacoes = aggregateEvent.NumeroAtualizacoes;
            _revisaoSolicitada = true;
        }

        public void Apply(TarefaSagaFinalizadaEvent aggregateEvent)
        {
            _numeroAtualizacoes = aggregateEvent.TotalAtualizacoes;
            _revisaoSolicitada = aggregateEvent.RevisaoSolicitada;
            _validacaoRealizada = aggregateEvent.ValidacaoRealizada;
            
            // Marca a saga como completa
            Complete();
        }
    }
}
