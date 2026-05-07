using OrderHub.Api.Domain;
using OrderHub.Api.Infrastructure.Persistence;

namespace OrderHub.Api.Features.Products.CreateProduct;

public record CreateProductRequest(string? Name);

public static class CreateProductEndpoint
{
    public static IEndpointRouteBuilder MapCreateProduct(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/products", (CreateProductRequest request, IProductStore store) =>
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return Results.BadRequest();

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = request.Name
            };

            store.Add(product);

            return Results.Created($"/api/products/{product.Id}", product);
        });

        return app;
    }
}
