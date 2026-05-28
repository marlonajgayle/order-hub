using OrderHub.Api.Infrastructure.Persistence;

namespace OrderHub.Api.Features.Products.ListProducts;

public static class ListProductsEndpoint
{
    public static IEndpointRouteBuilder MapListProducts(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/products", (IProductStore store) =>
        {
            var products = store.GetAll().ToList();
            return Results.Ok(products);
        });

        return app;
    }
}
