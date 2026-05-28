using OrderHub.Api.Infrastructure.Persistence;

namespace OrderHub.Api.Features.Products.DeleteProduct;

public static class DeleteProductEndpoint
{
    public static IEndpointRouteBuilder MapDeleteProduct(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/products/{id:guid}", (Guid id, IProductStore store) =>
        {
            var removed = store.Remove(id);
            return removed ? Results.NoContent() : Results.NotFound();
        });

        return app;
    }
}
