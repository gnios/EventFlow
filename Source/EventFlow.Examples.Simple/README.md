# Exemplo CQRS + Event Sourcing com EventFlow

Este Ã© um exemplo completo demonstrando as melhores prÃ¡ticas do .NET para CQRS e Event Sourcing.

## Como Executar

\\\ash
dotnet run --project Source/EventFlow.Examples.Simple/EventFlow.Examples.Simple.csproj
\\\

## Estrutura

- **Domain/Model/TarefaModel/**: Agregado, eventos, comandos e queries
- **ReadModels/**: Modelos de leitura otimizados
- **Configuration/**: ConfiguraÃ§Ã£o do EventFlow
