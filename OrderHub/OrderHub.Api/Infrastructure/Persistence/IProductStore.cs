using OrderHub.Api.Domain;

namespace OrderHub.Api.Infrastructure.Persistence;

public interface IProductStore
{
    IEnumerable<Product> GetAll();
    IEnumerable<string> GetCategories();
    IEnumerable<Product> GetByCategory(string category);
    void Add(Product product);
    bool Remove(Guid id);
    Product? Update(Guid id, string name, string description, decimal price);
}
