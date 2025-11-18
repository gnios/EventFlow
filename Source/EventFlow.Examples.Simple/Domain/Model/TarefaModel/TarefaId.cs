using EventFlow.Core;
using EventFlow.ValueObjects;
using Newtonsoft.Json;

namespace EventFlow.Examples.Simple.Domain.Model.TarefaModel
{
    [JsonConverter(typeof(SingleValueObjectConverter))]
    public class TarefaId : Identity<TarefaId>
    {
        public TarefaId(string value) : base(value)
        {
        }
    }
}
