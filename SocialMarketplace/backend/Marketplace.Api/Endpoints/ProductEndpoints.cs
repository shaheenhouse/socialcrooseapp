namespace Marketplace.Api.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products").WithTags("Products");

        // Get all products
        group.MapGet("/", () =>
        {
            // TODO: Implement product listing
            return Results.Ok(new { data = Array.Empty<object>(), pagination = new { page = 1, pageSize = 20, totalCount = 0 } });
        })
        .WithName("GetAllProducts")
        .WithSummary("Get all products with pagination and filters");

        // Get product by ID
        group.MapGet("/{id:guid}", (Guid id) =>
        {
            // TODO: Implement get product by ID
            return Results.NotFound();
        })
        .WithName("GetProductById")
        .WithSummary("Get product by ID");

        // Get product by slug
        group.MapGet("/slug/{slug}", (string slug) =>
        {
            // TODO: Implement get product by slug
            return Results.NotFound();
        })
        .WithName("GetProductBySlug")
        .WithSummary("Get product by slug");

        // Create product
        group.MapPost("/", () =>
        {
            // TODO: Implement create product
            return Results.Created();
        })
        .RequireAuthorization()
        .WithName("CreateProduct")
        .WithSummary("Create a new product");

        // Update product
        group.MapPatch("/{id:guid}", (Guid id) =>
        {
            // TODO: Implement update product
            return Results.Ok();
        })
        .RequireAuthorization()
        .WithName("UpdateProduct")
        .WithSummary("Update product");

        // Delete product
        group.MapDelete("/{id:guid}", (Guid id) =>
        {
            // TODO: Implement delete product
            return Results.NoContent();
        })
        .RequireAuthorization()
        .WithName("DeleteProduct")
        .WithSummary("Delete product");

        // Get product reviews
        group.MapGet("/{id:guid}/reviews", (Guid id) =>
        {
            // TODO: Implement get product reviews
            return Results.Ok(new { data = Array.Empty<object>() });
        })
        .WithName("GetProductReviews")
        .WithSummary("Get reviews for a product");
    }
}
