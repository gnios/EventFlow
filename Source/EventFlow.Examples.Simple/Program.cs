using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventFlow.Aggregates;
using EventFlow.Commands;
using EventFlow.Examples.Simple.Configuration;
using EventFlow.Examples.Simple.Domain.Model.BookingModel;
using EventFlow.Examples.Simple.Domain.Model.BookingModel.Commands;
using EventFlow.Examples.Simple.Domain.Model.BookingModel.Sagas;
using EventFlow.Examples.Simple.Domain.Model.OrderModel;
using EventFlow.Examples.Simple.Domain.Model.OrderModel.Commands;
using EventFlow.Examples.Simple.Domain.Model.OrderModel.Queries;
using EventFlow.Examples.Simple.Domain.Model.OrderModel.Sagas;
using EventFlow.Examples.Simple.Domain.Model.OrderModel.ValueObjects;
using EventFlow.Queries;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EventFlow.Examples.Simple
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Exemplo CQRS + Event Sourcing + Saga Orchestration ===\n");
            Console.WriteLine("Workflow: Criar Pedido → Reservar Estoque → Processar Pagamento → Finalizar\n");

            // Configuração do EventFlow
            var services = new ServiceCollection()
                .AddLogging(builder => builder
                    .AddConsole()
                    .SetMinimumLevel(LogLevel.Information))
                .AddEventFlowConfiguration();

            using var serviceProvider = services.BuildServiceProvider();

            var commandBus = serviceProvider.GetRequiredService<ICommandBus>();
            var queryProcessor = serviceProvider.GetRequiredService<IQueryProcessor>();
            var aggregateStore = serviceProvider.GetRequiredService<IAggregateStore>();

            // Criar um novo pedido
            var orderId = OrderId.New;
            Console.WriteLine($"1. Criando pedido com ID: {orderId}");

            var orderItems = new List<OrderItem>
            {
                new OrderItem("PROD-001", "Notebook", 1, 2500.00m),
                new OrderItem("PROD-002", "Mouse", 2, 50.00m)
            };

            await commandBus.PublishAsync(
                new CreateOrderCommand(
                    orderId,
                    "CUST-123",
                    "PAY-456",
                    orderItems),
                CancellationToken.None);

            Console.WriteLine("   ✓ Pedido criado com sucesso!");
            Console.WriteLine("   → Saga iniciou e publicou ReserveStockCommand\n");

            // Aguardar um pouco para a saga processar
            await Task.Delay(100);

            // Consultar o pedido
            var order = await queryProcessor.ProcessAsync(
                new GetOrderQuery(orderId),
                CancellationToken.None);

            Console.WriteLine("2. Estado do pedido após criação:");
            ExibirPedido(order);
            Console.WriteLine();

            // Aguardar processamento da saga
            await Task.Delay(100);

            // Consultar novamente após saga processar estoque
            order = await queryProcessor.ProcessAsync(
                new GetOrderQuery(orderId),
                CancellationToken.None);

            Console.WriteLine("3. Estado do pedido após reserva de estoque:");
            Console.WriteLine("   → Saga processou StockReserved e publicou CompletePaymentCommand");
            ExibirPedido(order);
            Console.WriteLine();

            // Aguardar processamento do pagamento
            await Task.Delay(100);

            // Consultar final
            order = await queryProcessor.ProcessAsync(
                new GetOrderQuery(orderId),
                CancellationToken.None);

            Console.WriteLine("4. Estado final do pedido:");
            Console.WriteLine("   → Saga processou PaymentCompleted e finalizou o workflow");
            ExibirPedido(order);
            Console.WriteLine();

            // Buscar e exibir a saga salva
            Console.WriteLine("5. Buscando saga salva no banco de dados:");
            await BuscarEExibirSaga(aggregateStore, orderId);
            Console.WriteLine();

            Console.WriteLine("=== Workflow executado com sucesso! ===");
            Console.WriteLine("\nEste exemplo demonstra:");
            Console.WriteLine("  • CQRS: Separação entre comandos (write) e queries (read)");
            Console.WriteLine("  • Event Sourcing: Histórico completo de eventos do pedido");
            Console.WriteLine("  • Saga Orchestration: OrderDeclarativeSaga coordenou workflow completo:");
            Console.WriteLine("    1. OrderCreated → Saga publicou ReserveStockCommand");
            Console.WriteLine("    2. StockReserved → Saga publicou CompletePaymentCommand");
            Console.WriteLine("    3. PaymentCompleted → Saga publicou MarkOrderCompletedCommand");
            Console.WriteLine("  • Tratamento de erros:");
            Console.WriteLine("    - StockReservationFailed → Saga marca pedido como falho");
            Console.WriteLine("    - PaymentFailed → Saga faz rollback de estoque e marca como falho");
            Console.WriteLine("\n  A saga demonstra coordenação real de workflows:");
            Console.WriteLine("  - Publica comandos baseados em eventos recebidos");
            Console.WriteLine("  - Mantém estado do processo (OrderCreated → StockReserved → PaymentCompleted)");
            Console.WriteLine("  - Coordena múltiplos passos do workflow");
            Console.WriteLine("  - Implementa compensação (rollback) em caso de falha");

            // ====================================================================
            // EXECUTAR SAGA DE BOOKING
            // ====================================================================
            Console.WriteLine("\n\n" + new string('=', 70));
            Console.WriteLine("=== EXECUTANDO SAGA DE BOOKING ===");
            Console.WriteLine(new string('=', 70) + "\n");
            Console.WriteLine("Workflow: Criar Reserva → Reservar Quarto → Processar Pagamento → Enviar Confirmação → Finalizar\n");

            // Criar uma nova reserva
            var bookingId = BookingId.New;
            Console.WriteLine($"1. Criando reserva com ID: {bookingId}");

            var checkInDate = DateTime.UtcNow.AddDays(7);
            var checkOutDate = DateTime.UtcNow.AddDays(10);

            await commandBus.PublishAsync(
                new CreateBookingCommand(
                    bookingId,
                    "CUST-789",
                    "HOTEL-001",
                    checkInDate,
                    checkOutDate,
                    1500.00m),
                CancellationToken.None);

            Console.WriteLine("   ✓ Reserva criada com sucesso!");
            Console.WriteLine("   → Saga iniciou e publicou ReserveRoomCommand\n");

            // Aguardar um pouco para a saga processar
            await Task.Delay(100);

            // Buscar e exibir a saga de booking
            Console.WriteLine("2. Buscando saga de booking salva no banco de dados:");
            await BuscarEExibirBookingSaga(aggregateStore, bookingId);
            Console.WriteLine();

            // Aguardar mais processamento
            await Task.Delay(200);

            // Buscar novamente para ver atualizações
            Console.WriteLine("3. Buscando saga novamente após processamento:");
            await BuscarEExibirBookingSaga(aggregateStore, bookingId);
            Console.WriteLine();

            Console.WriteLine("=== Workflow de Booking executado! ===");
            Console.WriteLine("\nEste exemplo demonstra a saga de Booking com:");
            Console.WriteLine("  • Múltiplos eventos iniciais (BookingCreated, BookingCreatedWithPromotion, LastMinuteBookingCreated)");
            Console.WriteLine("  • Múltiplos caminhos de sucesso");
            Console.WriteLine("  • Múltiplos caminhos de falha");
            Console.WriteLine("  • Timeouts com compensação");
            Console.WriteLine("  • Comandos agendados");
            Console.WriteLine("  • Ações encadeadas (Then, ThenAsync)");
        }

        private static void ExibirPedido(ReadModels.OrderReadModel order)
        {
            Console.WriteLine($"   ID: {order.Id}");
            Console.WriteLine($"   Cliente: {order.CustomerId}");
            Console.WriteLine($"   Conta de Pagamento: {order.PaymentAccountId}");
            Console.WriteLine($"   Status: {order.Status}");
            Console.WriteLine($"   Total: R$ {order.TotalPrice:F2}");
            Console.WriteLine($"   Data de Criação: {order.CreatedDate:yyyy-MM-dd HH:mm:ss}");
            
            Console.WriteLine($"   Itens ({order.OrderItems.Count}):");
            foreach (var item in order.OrderItems)
            {
                Console.WriteLine($"     - {item.ProductName} (Qtd: {item.Quantity}) - R$ {item.TotalPrice:F2}");
            }
            
            if (!string.IsNullOrEmpty(order.ErrorMessage))
            {
                Console.WriteLine($"   ⚠ Erro: {order.ErrorMessage}");
            }
        }

        private static async Task BuscarEExibirSaga(IAggregateStore aggregateStore, OrderId orderId)
        {
            try
            {
                // Criar o ID da saga usando o mesmo padrão do OrderSagaLocator
                var sagaId = new OrderSagaId($"order-saga-{orderId.Value}");
                
                Console.WriteLine($"   Buscando saga com ID: {sagaId.Value}");

                // Carregar a saga do banco de dados
                var saga = await aggregateStore.LoadAsync<OrderDeclarativeSaga, OrderSagaId>(
                    sagaId,
                    CancellationToken.None);

                if (saga == null)
                {
                    Console.WriteLine("   ⚠ Saga não encontrada no banco de dados");
                    return;
                }

                Console.WriteLine($"   ✓ Saga encontrada!");
                Console.WriteLine($"   Saga ID: {saga.Id.Value}");
                Console.WriteLine($"   Versão: {saga.Version}");
                Console.WriteLine($"   É Nova: {saga.IsNew}");
                Console.WriteLine($"   Estado: {saga.State}");
                
                // Exibir eventos não commitados (se houver)
                var uncommittedEvents = saga.UncommittedEvents;
                if (uncommittedEvents != null && uncommittedEvents.Any())
                {
                    Console.WriteLine($"   Eventos não commitados: {uncommittedEvents.Count()}");
                    foreach (var uncommittedEvent in uncommittedEvents)
                    {
                        Console.WriteLine($"     - {uncommittedEvent.GetType().Name}");
                    }
                }
                else
                {
                    Console.WriteLine($"   Eventos não commitados: 0 (todos foram persistidos)");
                }

                Console.WriteLine($"   ✓ Saga carregada e exibida com sucesso!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ⚠ Erro ao buscar saga: {ex.Message}");
                Console.WriteLine($"   Detalhes: {ex}");
            }
        }

        private static async Task BuscarEExibirBookingSaga(IAggregateStore aggregateStore, BookingId bookingId)
        {
            try
            {
                // Criar o ID da saga usando o mesmo padrão do BookingSagaLocator
                var sagaId = new BookingSagaId($"booking-saga-{bookingId.Value}");
                
                Console.WriteLine($"   Buscando saga com ID: {sagaId.Value}");

                // Carregar a saga do banco de dados
                var saga = await aggregateStore.LoadAsync<BookingSaga, BookingSagaId>(
                    sagaId,
                    CancellationToken.None);

                if (saga == null)
                {
                    Console.WriteLine("   ⚠ Saga não encontrada no banco de dados");
                    return;
                }

                Console.WriteLine($"   ✓ Saga encontrada!");
                Console.WriteLine($"   Saga ID: {saga.Id.Value}");
                Console.WriteLine($"   Versão: {saga.Version}");
                Console.WriteLine($"   É Nova: {saga.IsNew}");
                Console.WriteLine($"   Estado: {saga.State}");
                
                // Exibir eventos não commitados (se houver)
                var uncommittedEvents = saga.UncommittedEvents;
                if (uncommittedEvents != null && uncommittedEvents.Any())
                {
                    Console.WriteLine($"   Eventos não commitados: {uncommittedEvents.Count()}");
                    foreach (var uncommittedEvent in uncommittedEvents)
                    {
                        Console.WriteLine($"     - {uncommittedEvent.GetType().Name}");
                    }
                }
                else
                {
                    Console.WriteLine($"   Eventos não commitados: 0 (todos foram persistidos)");
                }

                Console.WriteLine($"   ✓ Saga carregada e exibida com sucesso!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ⚠ Erro ao buscar saga: {ex.Message}");
                Console.WriteLine($"   Detalhes: {ex}");
            }
        }
    }
}
