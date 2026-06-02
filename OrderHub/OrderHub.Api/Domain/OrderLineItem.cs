namespace OrderHub.Api.Domain;

public class OrderLineItem
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
