using OrderHub.Api.Infrastructure.Persistence;

namespace OrderHub.Api.Features.Products.EditProduct;

public record EditProductRequest(string? Name, string? Description);

public static class EditProductEndpoint
{
    public static IEndpointRouteBuilder MapEditProduct(this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/products/{id:guid}", (Guid id, EditProductRequest request, IProductStore store) =>
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return Results.BadRequest();

            var updated = store.Update(id, request.Name, request.Description ?? string.Empty);
            return updated is null ? Results.NotFound() : Results.Ok(updated);
        });

        return app;
    }
}
