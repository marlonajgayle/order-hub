using OrderHub.Api.Domain;

namespace OrderHub.Api.Infrastructure.Persistence;

public interface IProductStore
{
    IEnumerable<Product> GetAll();
    void Add(Product product);
    bool Remove(Guid id);
    Product? Update(Guid id, string name, string description);
}
