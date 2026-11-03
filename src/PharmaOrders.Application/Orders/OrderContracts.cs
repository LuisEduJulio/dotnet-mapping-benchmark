using PharmaOrders.Domain.Orders;

namespace PharmaOrders.Application.Orders;

/// <summary>Contrato de saída da API (achatado: CustomerName, CustomerCity, CustomerUf).</summary>
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

    // Calculado no próprio DTO: nenhum mapper faz trabalho extra (comparação justa).
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

/// <summary>Porta de saída: a Application não sabe COMO o mapeamento é feito.</summary>
public interface IOrderMapper
{
    string Name { get; }
    OrderDto Map(Order order);
    List<OrderDto> MapList(IReadOnlyList<Order> orders);
}

/// <summary>Porta de persistência.</summary>
public interface IOrderRepository
{
    IReadOnlyList<Order> ListPage(int page, int pageSize);
}

public sealed record ListOrdersQuery(int Page = 1, int PageSize = 20);

/// <summary>Caso de uso: listar pedidos já no formato da API.</summary>
public sealed class ListOrdersHandler(IOrderRepository repository, IOrderMapper mapper)
{
    public List<OrderDto> Handle(ListOrdersQuery query)
    {
        var page = Math.Max(1, query.Page);
        var size = Math.Clamp(query.PageSize, 1, 100);
        return mapper.MapList(repository.ListPage(page, size));
    }
}
