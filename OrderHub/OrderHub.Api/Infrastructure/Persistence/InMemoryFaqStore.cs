using OrderHub.Api.Domain;

namespace OrderHub.Api.Infrastructure.Persistence;

public class InMemoryFaqStore : IFaqStore
{
    private readonly List<Faq> _faqs = [];

    public IEnumerable<Faq> GetAll() => _faqs;

    public void Add(Faq faq) => _faqs.Add(faq);
}
