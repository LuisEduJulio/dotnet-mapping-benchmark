using AutoMapper;
using PharmaOrders.Application.Orders;
using PharmaOrders.Domain.Orders;
using Riok.Mapperly.Abstractions;

namespace PharmaOrders.Infrastructure.Mapping;

internal static class StatusNames
{
    // switch com nameof: sem reflection e sem alocação (Enum.ToString() é mais caro).
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
// 1) MANUAL: código escrito à mão. É a referência de performance.
// -------------------------------------------------------------------------------------------
public sealed class ManualOrderMapper : IOrderMapper
{
    public string Name => "Manual";

    public OrderDto Map(Order o)
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

    public List<OrderDto> MapList(IReadOnlyList<Order> orders)
    {
        var result = new List<OrderDto>(orders.Count);
        foreach (var o in orders) result.Add(Map(o));
        return result;
    }
}

// -------------------------------------------------------------------------------------------
// 2) CLASSE ABSTRATA: MapperBase<TSource, TDest> com métodos virtuais, padrão comum para
//    "padronizar" mappers no time. Custo extra: chamadas virtuais em cada item.
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

public sealed class AbstractClassOrderMapper(MapperBase<OrderItem, OrderItemDto> itemMapper)
    : MapperBase<Order, OrderDto>, IOrderMapper
{
    public string Name => "Classe abstrata";

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
// 3) AUTOMAPPER 14: convenção + reflection na configuração e expressões compiladas no runtime.
//    Achata CustomerName -> Customer.Name sozinho e converte enum em texto.
// -------------------------------------------------------------------------------------------
public sealed class AutoMapperOrderMapper : IOrderMapper
{
    private readonly IMapper _mapper;

    public AutoMapperOrderMapper()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Order, OrderDto>();
            cfg.CreateMap<OrderItem, OrderItemDto>();
        });
        config.AssertConfigurationIsValid();
        _mapper = config.CreateMapper();
    }

    public string Name => "AutoMapper 14";
    public OrderDto Map(Order order) => _mapper.Map<OrderDto>(order);
    public List<OrderDto> MapList(IReadOnlyList<Order> orders) => _mapper.Map<List<OrderDto>>(orders);
}

// -------------------------------------------------------------------------------------------
// 4) MAPPERLY: source generator. O código de mapeamento é GERADO NA COMPILAÇÃO:
//    sem reflection no runtime e erro de mapeamento vira aviso de build.
// -------------------------------------------------------------------------------------------
[Mapper]
public sealed partial class MapperlyOrderMapper : IOrderMapper
{
    public string Name => "Mapperly";

    [MapProperty("Customer.Name", nameof(OrderDto.CustomerName))]
    [MapProperty("Customer.City", nameof(OrderDto.CustomerCity))]
    [MapProperty("Customer.Uf", nameof(OrderDto.CustomerUf))]
    public partial OrderDto Map(Order order);

    public partial List<OrderDto> MapList(IReadOnlyList<Order> orders);
}
