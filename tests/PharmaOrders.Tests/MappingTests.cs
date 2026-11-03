using System.Text.Json;
using PharmaOrders.Application.Orders;
using PharmaOrders.Domain.Orders;
using PharmaOrders.Infrastructure;
using PharmaOrders.Infrastructure.Mapping;
using PharmaOrders.Infrastructure.Persistence;
using Xunit;

namespace PharmaOrders.Tests;

public sealed class MappingEquivalenceTests
{
    public static TheoryData<MappingStrategy> Strategies()
    {
        var data = new TheoryData<MappingStrategy>();
        foreach (var s in Enum.GetValues<MappingStrategy>()) data.Add(s);
        return data;
    }

    [Theory]
    [MemberData(nameof(Strategies))]
    public void Todas_as_estrategias_geram_o_mesmo_json_do_mapeamento_manual(MappingStrategy strategy)
    {
        var orders = SampleOrders.Create(50);
        var expected = JsonSerializer.Serialize(new ManualOrderMapper().MapList(orders));

        var actual = JsonSerializer.Serialize(DependencyInjection.CreateMapper(strategy).MapList(orders));

        Assert.Equal(expected, actual);
    }

    [Theory]
    [MemberData(nameof(Strategies))]
    public void Achata_cliente_converte_status_e_calcula_totais(MappingStrategy strategy)
    {
        var order = Order.Create(7, "PED-0000007", new DateTime(2026, 11, 3, 12, 0, 0, DateTimeKind.Utc),
            new Customer(1, "Maria", "maria@example.com", "Fortaleza", "ce"));
        order.AddItem(new OrderItem(10, "SKU-1", "Dipirona 500mg", quantity: 2, unitPrice: 10m, discount: 1m));
        order.AddItem(new OrderItem(11, "SKU-2", "Protetor solar", quantity: 1, unitPrice: 50m));
        order.ChangeStatus(OrderStatus.Shipped);

        var dto = DependencyInjection.CreateMapper(strategy).Map(order);

        Assert.Equal("Maria", dto.CustomerName);
        Assert.Equal("Fortaleza", dto.CustomerCity);
        Assert.Equal("CE", dto.CustomerUf);
        Assert.Equal("Shipped", dto.Status);
        Assert.Equal(2, dto.Items.Count);
        Assert.Equal(19m, dto.Items[0].Total);
        Assert.Equal(69m, dto.Total);
    }
}

public sealed class ListOrdersHandlerTests
{
    [Fact]
    public void Lista_a_pagina_pedida_usando_a_porta_de_mapeamento()
    {
        var handler = new ListOrdersHandler(new InMemoryOrderRepository(), new MapperlyOrderMapper());

        var page2 = handler.Handle(new ListOrdersQuery(Page: 2, PageSize: 20));

        Assert.Equal(20, page2.Count);
        Assert.Equal(21, page2[0].Id);
    }
}

public sealed class OrderDomainTests
{
    private static Order NewOrder() => Order.Create(1, "PED-1", DateTime.UtcNow,
        new Customer(1, "João", "joao@example.com", "Fortaleza", "CE"));

    [Fact]
    public void Nao_aceita_item_com_quantidade_zero() =>
        Assert.Throws<DomainException>(() => new OrderItem(1, "SKU", "Produto", quantity: 0, unitPrice: 10m));

    [Fact]
    public void Nao_aceita_desconto_maior_que_o_valor_do_item() =>
        Assert.Throws<DomainException>(() => new OrderItem(1, "SKU", "Produto", quantity: 1, unitPrice: 10m, discount: 11m));

    [Fact]
    public void Pedido_finalizado_nao_muda_de_status()
    {
        var order = NewOrder();
        order.ChangeStatus(OrderStatus.Delivered);
        Assert.Throws<DomainException>(() => order.ChangeStatus(OrderStatus.Cancelled));
    }

    [Fact]
    public void Nao_adiciona_item_depois_que_o_pedido_saiu_de_Created()
    {
        var order = NewOrder();
        order.ChangeStatus(OrderStatus.PaymentApproved);
        Assert.Throws<DomainException>(() => order.AddItem(new OrderItem(1, "SKU", "Produto", 1, 10m)));
    }
}
