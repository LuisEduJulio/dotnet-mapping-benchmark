using PharmaOrders.Application.Orders;
using PharmaOrders.Domain.Orders;

namespace PharmaOrders.Infrastructure.Persistence;

/// <summary>Repositório em memória com massa determinística (o foco aqui é o mapeamento, não o banco).</summary>
public sealed class InMemoryOrderRepository(IReadOnlyList<Order> orders) : IOrderRepository
{
    public InMemoryOrderRepository() : this(SampleOrders.Create(1_000)) { }

    public IReadOnlyList<Order> ListPage(int page, int pageSize) =>
        orders.Skip((page - 1) * pageSize).Take(pageSize).ToList();
}

public static class SampleOrders
{
    private static readonly string[] Ufs = ["CE", "SP", "RJ", "BA", "PE", "MG"];

    /// <summary>Gera pedidos determinísticos, cada um com 5 itens.</summary>
    public static List<Order> Create(int count, int itemsPerOrder = 5)
    {
        var orders = new List<Order>(count);
        var start = new DateTime(2026, 11, 3, 12, 0, 0, DateTimeKind.Utc);

        for (var i = 1; i <= count; i++)
        {
            var customer = new Customer(i, $"Cliente {i}", $"cliente{i}@example.com", "Fortaleza", Ufs[i % Ufs.Length]);
            var order = Order.Create(i, $"PED-{i:0000000}", start.AddMinutes(i), customer);

            for (var j = 1; j <= itemsPerOrder; j++)
            {
                order.AddItem(new OrderItem(
                    productId: i * 10 + j,
                    sku: $"SKU-{j:000000}",
                    name: $"Produto {j}",
                    quantity: 1 + (i + j) % 4,
                    unitPrice: 9.90m + j,
                    discount: j % 2 == 0 ? 1.50m : 0m));
            }

            var status = (OrderStatus)(i % 6);
            if (status != OrderStatus.Created) order.ChangeStatus(status);
            orders.Add(order);
        }

        return orders;
    }
}
