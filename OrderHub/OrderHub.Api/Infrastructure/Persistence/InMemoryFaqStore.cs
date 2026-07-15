using OrderHub.Api.Domain;

namespace OrderHub.Api.Infrastructure.Persistence;

public class InMemoryFaqStore : IFaqStore
{
    private readonly List<Faq> _faqs =
    [
        new Faq
        {
            Id = Guid.NewGuid(),
            Question = "How do I reset my password?",
            Answer = "Go to the login page and click \"Forgot password\" to receive a reset link by email."
        },
        new Faq
        {
            Id = Guid.NewGuid(),
            Question = "What payment methods are accepted?",
            Answer = "We accept all major credit cards, PayPal, and bank transfers."
        },
        new Faq
        {
            Id = Guid.NewGuid(),
            Question = "How long does shipping take?",
            Answer = "Standard shipping takes 3-5 business days. Express shipping arrives in 1-2 business days."
        },
        new Faq
        {
            Id = Guid.NewGuid(),
            Question = "Can I return a product?",
            Answer = "Yes, products can be returned within 30 days of delivery for a full refund."
        },
        new Faq
        {
            Id = Guid.NewGuid(),
            Question = "How do I track my order?",
            Answer = "Use the tracking number from your shipping confirmation email on the carrier's website."
        }
    ];

    public IEnumerable<Faq> GetAll() => _faqs;

    public IEnumerable<Faq> Search(string term)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(term);
        return _faqs.Where(f =>
            f.Question.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            f.Answer.Contains(term, StringComparison.OrdinalIgnoreCase));
    }

    public void Add(Faq faq) => _faqs.Add(faq);
}
