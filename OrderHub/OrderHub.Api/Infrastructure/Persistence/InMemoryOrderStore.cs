using OrderHub.Api.Domain;

namespace OrderHub.Api.Infrastructure.Persistence;

public class InMemoryOrderStore : IOrderStore
{
    private readonly List<Order> _orders = [];

    public void Add(Order order) => _orders.Add(order);

    public Order? GetById(Guid id) => _orders.FirstOrDefault(o => o.Id == id);
}
