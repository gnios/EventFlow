using System;
using System.Reflection;
using EventFlow.Commands;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace EventFlow.Examples.Simple.Configuration
{
    /// <summary>
    /// ContractResolver que ignora a propriedade ISourceId durante a serialização/deserialização de comandos.
    /// Isso resolve o problema de deserialização de interfaces quando comandos são serializados pelo PublishCommandJob.
    /// Durante a deserialização, o construtor padrão de Command já cria um novo SourceId automaticamente.
    /// </summary>
    public class IgnoreSourceIdContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            
            // Ignora SourceId durante serialização e deserialização
            if (property.PropertyName == "SourceId" && member.DeclaringType != null && 
                typeof(ICommand).IsAssignableFrom(member.DeclaringType))
            {
                property.ShouldSerialize = _ => false;
                property.Ignored = true;
            }
            
            return property;
        }
    }
}

