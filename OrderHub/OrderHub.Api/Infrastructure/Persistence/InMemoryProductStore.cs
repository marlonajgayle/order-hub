using OrderHub.Api.Domain;

namespace OrderHub.Api.Infrastructure.Persistence;

public class InMemoryProductStore : IProductStore
{
    private readonly List<Product> _products = [];

    public IEnumerable<Product> GetAll() => _products;

    public void Add(Product product) => _products.Add(product);

    public bool Remove(Guid id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        return product is not null && _products.Remove(product);
    }
}
