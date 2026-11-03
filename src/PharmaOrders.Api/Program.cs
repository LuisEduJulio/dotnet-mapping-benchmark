using PharmaOrders.Application.Orders;
using PharmaOrders.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddInfrastructure();

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new
{
    endpoints = new[]
    {
        "/orders?mapper=Manual&page=1&pageSize=20",
        "/orders?mapper=AbstractClass",
        "/orders?mapper=AutoMapper",
        "/orders?mapper=Mapperly"
    }
}));

// Mesmo caso de uso, trocando só a implementação do IOrderMapper (porta da Application).
app.MapGet("/orders", (IServiceProvider sp, IOrderRepository repository, MappingStrategy? mapper, int? page, int? pageSize) =>
{
    var strategy = mapper ?? MappingStrategy.Mapperly;
    var handler = new ListOrdersHandler(repository, sp.GetRequiredKeyedService<IOrderMapper>(strategy));
    return Results.Ok(handler.Handle(new ListOrdersQuery(page ?? 1, pageSize ?? 20)));
});

app.Run();

public partial class Program;
