using EventFlow.Aggregates;
using EventFlow.DeclarativeSaga.StateMachine;
using EventFlow.Examples.Simple.Domain.Model.OrderModel.Sagas.Events;

namespace EventFlow.Examples.Simple.Domain.Model.OrderModel.Sagas
{
    /// <summary>
    /// Estado agregado da saga declarativa.
    /// Herda de DeclarativeSagaState que gerencia automaticamente o CompensationJobId.
    /// Desenvolvedores só precisam aplicar seus próprios eventos de domínio - sem se preocupar com job IDs.
    /// </summary>
    public class OrderDeclarativeSagaState : DeclarativeSagaState<OrderDeclarativeSaga, OrderSagaId, OrderDeclarativeSagaState>,
        IApply<OrderDeclarativeSagaStartedEvent>,
        IApply<OrderDeclarativeSagaCompletedEvent>,
        IApply<OrderDeclarativeSagaFailedEvent>
    {
        // Propriedades para rastrear o estado da saga baseado nos eventos aplicados
        public bool IsStarted { get; private set; }
        public bool IsCompleted { get; private set; }
        public bool IsFailed { get; private set; }
        public string? FailureReason { get; private set; }

        public void Apply(OrderDeclarativeSagaStartedEvent aggregateEvent)
        {
            // Fato: A saga foi iniciada
            IsStarted = true;
        }

        public void Apply(OrderDeclarativeSagaCompletedEvent aggregateEvent)
        {
            // Fato: A saga foi completada com sucesso
            IsCompleted = true;
        }

        public void Apply(OrderDeclarativeSagaFailedEvent aggregateEvent)
        {
            // Fato: A saga falhou
            IsFailed = true;
            FailureReason = aggregateEvent.ErrorMessage;
        }
    }
}

