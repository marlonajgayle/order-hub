using OrderHub.Api.Domain;

namespace OrderHub.Api.Infrastructure.Persistence;

public interface IOrderStore
{
    void Add(Order order);
    Order? GetById(Guid id);
}
