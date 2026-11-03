namespace MappingBenchmarks.Domain;

public enum OrderStatus
{
    Created,
    PaymentApproved,
    Picking,
    Shipped,
    Delivered,
    Cancelled
}

/// <summary>Pedido de uma rede de farmácias (entidade de domínio).</summary>
public sealed class Order
{
    public long Id { get; set; }
    public string Number { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public OrderStatus Status { get; set; }
    public Customer Customer { get; set; } = default!;
    public List<OrderItem> Items { get; set; } = [];
}

public sealed class Customer
{
    public long Id { get; set; }
    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string City { get; set; } = default!;
    public string Uf { get; set; } = default!;
}

public sealed class OrderItem
{
    public long ProductId { get; set; }
    public string Sku { get; set; } = default!;
    public string Name { get; set; } = default!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
}

/// <summary>Contrato da API (achatado: CustomerName, CustomerCity, CustomerUf).</summary>
public sealed class OrderDto
{
    public long Id { get; set; }
    public string Number { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = default!;
    public string CustomerName { get; set; } = default!;
    public string CustomerCity { get; set; } = default!;
    public string CustomerUf { get; set; } = default!;
    public List<OrderItemDto> Items { get; set; } = [];

    // Calculado no DTO para que nenhum mapper tenha trabalho extra (comparação justa).
    public decimal Total => Items.Sum(i => i.Total);
}

public sealed class OrderItemDto
{
    public string Sku { get; set; } = default!;
    public string Name { get; set; } = default!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }

    public decimal Total => Quantity * UnitPrice - Discount;
}

public static class SampleData
{
    private static readonly string[] Ufs = ["CE", "SP", "RJ", "BA", "PE", "MG"];

    /// <summary>Gera pedidos determinísticos: cada um com 5 itens.</summary>
    public static List<Order> Orders(int count, int itemsPerOrder = 5)
    {
        var orders = new List<Order>(count);
        for (var i = 1; i <= count; i++)
        {
            var items = new List<OrderItem>(itemsPerOrder);
            for (var j = 1; j <= itemsPerOrder; j++)
            {
                items.Add(new OrderItem
                {
                    ProductId = i * 10 + j,
                    Sku = $"SKU-{j:000000}",
                    Name = $"Produto {j}",
                    Quantity = 1 + (i + j) % 4,
                    UnitPrice = 9.90m + j,
                    Discount = j % 2 == 0 ? 1.50m : 0m
                });
            }

            orders.Add(new Order
            {
                Id = i,
                Number = $"PED-{i:0000000}",
                CreatedAt = new DateTime(2026, 11, 3, 12, 0, 0, DateTimeKind.Utc).AddMinutes(i),
                Status = (OrderStatus)(i % 6),
                Customer = new Customer
                {
                    Id = i,
                    Name = $"Cliente {i}",
                    Email = $"cliente{i}@example.com",
                    City = "Fortaleza",
                    Uf = Ufs[i % Ufs.Length]
                },
                Items = items
            });
        }
        return orders;
    }
}
