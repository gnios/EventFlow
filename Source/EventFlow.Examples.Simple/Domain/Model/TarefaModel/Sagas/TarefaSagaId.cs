using EventFlow.Sagas;
using EventFlow.ValueObjects;

namespace EventFlow.Examples.Simple.Domain.Model.TarefaModel.Sagas
{
    public class TarefaSagaId : SingleValueObject<string>, ISagaId
    {
        public TarefaSagaId(string value) : base(value)
        {
        }
    }
}

