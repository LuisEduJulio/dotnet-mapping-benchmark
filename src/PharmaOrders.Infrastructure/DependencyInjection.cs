using Microsoft.Extensions.DependencyInjection;
using PharmaOrders.Application.Orders;
using PharmaOrders.Infrastructure.Mapping;
using PharmaOrders.Infrastructure.Persistence;

namespace PharmaOrders.Infrastructure;

public enum MappingStrategy
{
    Manual,
    AbstractClass,
    AutoMapper,
    Mapperly
}

public static class DependencyInjection
{
    /// <summary>Todas as estratégias disponíveis, na ordem do benchmark.</summary>
    public static IReadOnlyList<IOrderMapper> AllMappers() =>
    [
        new ManualOrderMapper(),
        new AbstractClassOrderMapper(new OrderItemAbstractMapper()),
        new AutoMapperOrderMapper(),
        new MapperlyOrderMapper()
    ];

    public static IOrderMapper CreateMapper(MappingStrategy strategy) => strategy switch
    {
        MappingStrategy.Manual => new ManualOrderMapper(),
        MappingStrategy.AbstractClass => new AbstractClassOrderMapper(new OrderItemAbstractMapper()),
        MappingStrategy.AutoMapper => new AutoMapperOrderMapper(),
        MappingStrategy.Mapperly => new MapperlyOrderMapper(),
        _ => throw new ArgumentOutOfRangeException(nameof(strategy))
    };

    /// <summary>Registra repositório, caso de uso e todos os mappers (singleton: configurados uma única vez).</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();
        foreach (var strategy in Enum.GetValues<MappingStrategy>())
            services.AddKeyedSingleton<IOrderMapper>(strategy, (_, _) => CreateMapper(strategy));
        return services;
    }
}
