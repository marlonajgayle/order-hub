using OrderHub.Api.Domain;

namespace OrderHub.Api.Infrastructure.Persistence;

public class InMemoryProductStore : IProductStore
{
    private readonly List<Product> _products = [];

    public IEnumerable<Product> GetAll() => _products;

    public IEnumerable<string> GetCategories() =>
        _products
            .Select(p => p.Category)
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Select(c => c!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(c => c, StringComparer.OrdinalIgnoreCase);

    public IEnumerable<Product> GetByCategory(string category)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(category);
        return _products.Where(p => string.Equals(p.Category, category, StringComparison.OrdinalIgnoreCase));
    }

    public void Add(Product product) => _products.Add(product);

    public bool Remove(Guid id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        return product is not null && _products.Remove(product);
    }

    public Product? Update(Guid id, string name, string description)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product is null) return null;
        product.Name = name;
        product.Description = description;
        return product;
    }
}
