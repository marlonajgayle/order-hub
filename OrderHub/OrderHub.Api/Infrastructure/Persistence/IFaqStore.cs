using OrderHub.Api.Domain;

namespace OrderHub.Api.Infrastructure.Persistence;

public interface IFaqStore
{
    IEnumerable<Faq> GetAll();
    void Add(Faq faq);
}
