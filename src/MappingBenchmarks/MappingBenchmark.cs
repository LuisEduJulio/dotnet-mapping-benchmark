using AutoMapper;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using MappingBenchmarks.Domain;
using MappingBenchmarks.Mappers;

namespace MappingBenchmarks;

/// <summary>
/// Mapeia uma lista de pedidos (cada um com cliente achatado e 5 itens) para DTO.
/// 1 pedido = uma requisição de detalhe; 100 = uma página de listagem; 1.000 = um export/relatório.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class MappingBenchmark
{
    private List<Order> _orders = default!;
    private IMapper _autoMapper = default!;
    private MapperBase<Order, OrderDto> _abstractMapper = default!;

    [Params(1, 100, 1000)]
    public int Orders { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _orders = SampleData.Orders(Orders);
        _autoMapper = AutoMapperSetup.Create();
        // injetado pelo tipo base, como num container de DI
        _abstractMapper = new OrderAbstractMapper(new OrderItemAbstractMapper());
    }

    [Benchmark(Baseline = true, Description = "Manual")]
    public List<OrderDto> Manual() => ManualMapping.ToDtos(_orders);

    [Benchmark(Description = "Classe abstrata")]
    public List<OrderDto> AbstractClass() => _abstractMapper.MapList(_orders);

    [Benchmark(Description = "AutoMapper 14")]
    public List<OrderDto> AutoMapper() => _autoMapper.Map<List<OrderDto>>(_orders);

    [Benchmark(Description = "Mapperly (source generator)")]
    public List<OrderDto> Mapperly() => MapperlyMapping.ToDtos(_orders);
}
