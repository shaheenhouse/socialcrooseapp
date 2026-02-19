namespace Marketplace.Api.Endpoints;

public static class StoreEndpoints
{
    public static void MapStoreEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/stores").WithTags("Stores");

        // Get all stores
        group.MapGet("/", () =>
        {
            // TODO: Implement store listing
            return Results.Ok(new { data = Array.Empty<object>(), pagination = new { page = 1, pageSize = 20, totalCount = 0 } });
        })
        .WithName("GetAllStores")
        .WithSummary("Get all stores with pagination");

        // Get store by ID
        group.MapGet("/{id:guid}", (Guid id) =>
        {
            // TODO: Implement get store by ID
            return Results.NotFound();
        })
        .WithName("GetStoreById")
        .WithSummary("Get store by ID");

        // Get store by slug
        group.MapGet("/slug/{slug}", (string slug) =>
        {
            // TODO: Implement get store by slug
            return Results.NotFound();
        })
        .WithName("GetStoreBySlug")
        .WithSummary("Get store by slug");

        // Create store
        group.MapPost("/", () =>
        {
            // TODO: Implement create store
            return Results.Created();
        })
        .RequireAuthorization()
        .WithName("CreateStore")
        .WithSummary("Create a new store");

        // Update store
        group.MapPatch("/{id:guid}", (Guid id) =>
        {
            // TODO: Implement update store
            return Results.Ok();
        })
        .RequireAuthorization()
        .WithName("UpdateStore")
        .WithSummary("Update store");

        // Get store products
        group.MapGet("/{id:guid}/products", (Guid id) =>
        {
            // TODO: Implement get store products
            return Results.Ok(new { data = Array.Empty<object>() });
        })
        .WithName("GetStoreProducts")
        .WithSummary("Get products for a store");

        // Get store employees
        group.MapGet("/{id:guid}/employees", (Guid id) =>
        {
            // TODO: Implement get store employees
            return Results.Ok(new { data = Array.Empty<object>() });
        })
        .RequireAuthorization()
        .WithName("GetStoreEmployees")
        .WithSummary("Get employees for a store");
    }
}
