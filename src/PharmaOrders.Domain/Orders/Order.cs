namespace PharmaOrders.Domain.Orders;

public enum OrderStatus
{
    Created,
    PaymentApproved,
    Picking,
    Shipped,
    Delivered,
    Cancelled
}

/// <summary>Exceção para violações de regra de negócio do domínio.</summary>
public sealed class DomainException(string message) : Exception(message);

/// <summary>Aggregate root: o pedido controla seus itens e suas regras.</summary>
public sealed class Order
{
    private readonly List<OrderItem> _items = [];

    private Order(long id, string number, DateTime createdAt, Customer customer)
    {
        Id = id;
        Number = number;
        CreatedAt = createdAt;
        Customer = customer;
        Status = OrderStatus.Created;
    }

    public long Id { get; }
    public string Number { get; }
    public DateTime CreatedAt { get; }
    public OrderStatus Status { get; private set; }
    public Customer Customer { get; }
    public IReadOnlyList<OrderItem> Items => _items;

    public static Order Create(long id, string number, DateTime createdAtUtc, Customer customer)
    {
        if (id <= 0) throw new DomainException("Id do pedido deve ser positivo.");
        if (string.IsNullOrWhiteSpace(number)) throw new DomainException("Número do pedido é obrigatório.");
        ArgumentNullException.ThrowIfNull(customer);
        return new Order(id, number, createdAtUtc, customer);
    }

    public void AddItem(OrderItem item)
    {
        if (Status is not OrderStatus.Created)
            throw new DomainException("Só é possível adicionar itens a um pedido recém-criado.");
        _items.Add(item);
    }

    public void ChangeStatus(OrderStatus status)
    {
        if (Status is OrderStatus.Cancelled or OrderStatus.Delivered)
            throw new DomainException($"Pedido {Number} já foi finalizado ({Status}).");
        Status = status;
    }
}

public sealed class Customer
{
    public Customer(long id, string name, string email, string city, string uf)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Nome do cliente é obrigatório.");
        if (uf is not { Length: 2 }) throw new DomainException("UF deve ter 2 letras.");
        Id = id;
        Name = name;
        Email = email;
        City = city;
        Uf = uf.ToUpperInvariant();
    }

    public long Id { get; }
    public string Name { get; }
    public string Email { get; }
    public string City { get; }
    public string Uf { get; }
}

/// <summary>Item do pedido (value object: imutável e validado na criação).</summary>
public sealed class OrderItem
{
    public OrderItem(long productId, string sku, string name, int quantity, decimal unitPrice, decimal discount = 0m)
    {
        if (quantity <= 0) throw new DomainException("Quantidade deve ser maior que zero.");
        if (unitPrice < 0) throw new DomainException("Preço não pode ser negativo.");
        if (discount < 0 || discount > quantity * unitPrice) throw new DomainException("Desconto inválido.");
        ProductId = productId;
        Sku = sku;
        Name = name;
        Quantity = quantity;
        UnitPrice = unitPrice;
        Discount = discount;
    }

    public long ProductId { get; }
    public string Sku { get; }
    public string Name { get; }
    public int Quantity { get; }
    public decimal UnitPrice { get; }
    public decimal Discount { get; }
}
