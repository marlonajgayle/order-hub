using OrderHub.Api.Domain;

namespace OrderHub.Api.Infrastructure.Persistence;

public interface IFaqStore
{
    IEnumerable<Faq> GetAll();
    IEnumerable<Faq> Search(string term);
    void Add(Faq faq);
}
