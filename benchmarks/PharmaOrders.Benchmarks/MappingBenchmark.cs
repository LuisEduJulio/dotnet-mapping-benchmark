using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using PharmaOrders.Application.Orders;
using PharmaOrders.Domain.Orders;
using PharmaOrders.Infrastructure;
using PharmaOrders.Infrastructure.Persistence;

namespace PharmaOrders.Benchmarks;

/// <summary>
/// Mapeia uma lista de pedidos (cliente achatado + 5 itens) para DTO.
/// 1 pedido = tela de detalhe; 100 = página de listagem; 1.000 = export/relatório.
/// Todas as estratégias são chamadas pela mesma porta (IOrderMapper), como numa API real com DI.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class MappingBenchmark
{
    private List<Order> _orders = default!;
    private IOrderMapper _manual = default!, _abstract = default!, _autoMapper = default!, _mapperly = default!;

    [Params(1, 100, 1000)]
    public int Orders { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _orders = SampleOrders.Create(Orders);
        _manual = DependencyInjection.CreateMapper(MappingStrategy.Manual);
        _abstract = DependencyInjection.CreateMapper(MappingStrategy.AbstractClass);
        _autoMapper = DependencyInjection.CreateMapper(MappingStrategy.AutoMapper);
        _mapperly = DependencyInjection.CreateMapper(MappingStrategy.Mapperly);
    }

    [Benchmark(Baseline = true, Description = "Manual")]
    public List<OrderDto> Manual() => _manual.MapList(_orders);

    [Benchmark(Description = "Classe abstrata")]
    public List<OrderDto> AbstractClass() => _abstract.MapList(_orders);

    [Benchmark(Description = "AutoMapper 14")]
    public List<OrderDto> AutoMapper() => _autoMapper.MapList(_orders);

    [Benchmark(Description = "Mapperly")]
    public List<OrderDto> Mapperly() => _mapperly.MapList(_orders);
}
