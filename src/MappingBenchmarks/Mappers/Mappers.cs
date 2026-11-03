using AutoMapper;
using MappingBenchmarks.Domain;
using Riok.Mapperly.Abstractions;

namespace MappingBenchmarks.Mappers;

internal static class StatusNames
{
    // switch com nameof: sem reflection e sem alocação (Enum.ToString() aloca e é lento).
    public static string Of(OrderStatus status) => status switch
    {
        OrderStatus.Created => nameof(OrderStatus.Created),
        OrderStatus.PaymentApproved => nameof(OrderStatus.PaymentApproved),
        OrderStatus.Picking => nameof(OrderStatus.Picking),
        OrderStatus.Shipped => nameof(OrderStatus.Shipped),
        OrderStatus.Delivered => nameof(OrderStatus.Delivered),
        OrderStatus.Cancelled => nameof(OrderStatus.Cancelled),
        _ => status.ToString()
    };
}

// -------------------------------------------------------------------------------------------
// 1) MANUAL — método estático escrito à mão. É o "teto" de performance: o código mais direto possível.
// -------------------------------------------------------------------------------------------
public static class ManualMapping
{
    public static OrderDto ToDto(Order o)
    {
        var items = new List<OrderItemDto>(o.Items.Count);
        foreach (var i in o.Items)
        {
            items.Add(new OrderItemDto
            {
                Sku = i.Sku,
                Name = i.Name,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Discount = i.Discount
            });
        }

        return new OrderDto
        {
            Id = o.Id,
            Number = o.Number,
            CreatedAt = o.CreatedAt,
            Status = StatusNames.Of(o.Status),
            CustomerName = o.Customer.Name,
            CustomerCity = o.Customer.City,
            CustomerUf = o.Customer.Uf,
            Items = items
        };
    }

    public static List<OrderDto> ToDtos(List<Order> orders)
    {
        var result = new List<OrderDto>(orders.Count);
        foreach (var o in orders) result.Add(ToDto(o));
        return result;
    }
}

// -------------------------------------------------------------------------------------------
// 2) CLASSE ABSTRATA — padrão corporativo comum: um MapperBase<TSource, TDest> com métodos
//    virtuais, injetado por DI pelo tipo base. Custo extra: chamadas virtuais (sem devirtualização).
// -------------------------------------------------------------------------------------------
public abstract class MapperBase<TSource, TDest>
{
    public abstract TDest Map(TSource source);

    public virtual List<TDest> MapList(IReadOnlyList<TSource> source)
    {
        var result = new List<TDest>(source.Count);
        for (var i = 0; i < source.Count; i++) result.Add(Map(source[i]));
        return result;
    }
}

public sealed class OrderItemAbstractMapper : MapperBase<OrderItem, OrderItemDto>
{
    public override OrderItemDto Map(OrderItem i) => new()
    {
        Sku = i.Sku,
        Name = i.Name,
        Quantity = i.Quantity,
        UnitPrice = i.UnitPrice,
        Discount = i.Discount
    };
}

public sealed class OrderAbstractMapper(MapperBase<OrderItem, OrderItemDto> itemMapper) : MapperBase<Order, OrderDto>
{
    public override OrderDto Map(Order o) => new()
    {
        Id = o.Id,
        Number = o.Number,
        CreatedAt = o.CreatedAt,
        Status = StatusNames.Of(o.Status),
        CustomerName = o.Customer.Name,
        CustomerCity = o.Customer.City,
        CustomerUf = o.Customer.Uf,
        Items = itemMapper.MapList(o.Items)
    };
}

// -------------------------------------------------------------------------------------------
// 3) AUTOMAPPER 14 — convenção + reflection na configuração, expressões compiladas no runtime.
//    Achata CustomerName -> Customer.Name sozinho e converte enum -> string com ToString().
// -------------------------------------------------------------------------------------------
public static class AutoMapperSetup
{
    public static IMapper Create()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Order, OrderDto>();
            cfg.CreateMap<OrderItem, OrderItemDto>();
        });
        config.AssertConfigurationIsValid();
        return config.CreateMapper();
    }
}

// -------------------------------------------------------------------------------------------
// 4) MAPPERLY — source generator: o código de mapeamento é GERADO NA COMPILAÇÃO.
//    Zero reflection no runtime, enum -> string vira switch, e erro de mapeamento vira warning de build.
// -------------------------------------------------------------------------------------------
[Mapper]
public static partial class MapperlyMapping
{
    [MapProperty("Customer.Name", nameof(OrderDto.CustomerName))]
    [MapProperty("Customer.City", nameof(OrderDto.CustomerCity))]
    [MapProperty("Customer.Uf", nameof(OrderDto.CustomerUf))]
    public static partial OrderDto ToDto(Order order);

    public static partial List<OrderDto> ToDtos(List<Order> orders);
}
