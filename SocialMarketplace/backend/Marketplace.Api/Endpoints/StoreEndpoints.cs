using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Marketplace.Slices.StoreSlice;

namespace Marketplace.Api.Endpoints;

public static class StoreEndpoints
{
    public static void MapStoreEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/stores").WithTags("Stores");

        group.MapGet("/", async (
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] string? status = null,
            IStoreService storeService) =>
        {
            var (stores, totalCount) = await storeService.GetAllAsync(page, pageSize, search, status);
            return Results.Ok(new
            {
                data = stores,
                pagination = new { page, pageSize, totalCount, totalPages = (int)Math.Ceiling(totalCount / (double)pageSize) }
            });
        })
        .WithName("GetAllStores")
        .WithSummary("Get all stores with pagination");

        group.MapGet("/{id:guid}", async (Guid id, IStoreService storeService) =>
        {
            var store = await storeService.GetByIdAsync(id);
            return store != null ? Results.Ok(store) : Results.NotFound();
        })
        .WithName("GetStoreById");

        group.MapGet("/slug/{slug}", async (string slug, IStoreService storeService) =>
        {
            var store = await storeService.GetBySlugAsync(slug);
            return store != null ? Results.Ok(store) : Results.NotFound();
        })
        .WithName("GetStoreBySlug");

        group.MapGet("/my", async (HttpContext context, IStoreService storeService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var store = await storeService.GetMyStoreAsync(userId.Value);
            return store != null ? Results.Ok(store) : Results.NotFound();
        })
        .RequireAuthorization()
        .WithName("GetMyStore");

        group.MapPost("/", async (HttpContext context, [FromBody] CreateStoreDto dto, IStoreService storeService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            try
            {
                var id = await storeService.CreateAsync(dto, userId.Value);
                return Results.Created($"/api/stores/{id}", new { id });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .RequireAuthorization()
        .WithName("CreateStore");

        group.MapPatch("/{id:guid}", async (HttpContext context, Guid id, [FromBody] UpdateStoreDto dto, IStoreService storeService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var result = await storeService.UpdateAsync(id, userId.Value, dto);
            return result ? Results.Ok(new { message = "Store updated" }) : Results.NotFound();
        })
        .RequireAuthorization()
        .WithName("UpdateStore");

        group.MapGet("/{id:guid}/products", async (Guid id,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            Slices.ProductSlice.IProductService productService) =>
        {
            var products = await productService.GetByStoreIdAsync(id, page, pageSize);
            return Results.Ok(new { data = products });
        })
        .WithName("GetStoreProducts");

        group.MapGet("/{id:guid}/employees", async (Guid id, IStoreService storeService) =>
        {
            var employees = await storeService.GetEmployeesAsync(id);
            return Results.Ok(new { data = employees });
        })
        .RequireAuthorization()
        .WithName("GetStoreEmployees");

        group.MapGet("/{id:guid}/analytics", async (HttpContext context, Guid id, IStoreService storeService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            try
            {
                var analytics = await storeService.GetAnalyticsAsync(id, userId.Value);
                return Results.Ok(analytics);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Forbid();
            }
        })
        .RequireAuthorization()
        .WithName("GetStoreAnalytics");
    }

    private static Guid? GetUserId(HttpContext context)
    {
        var claim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return claim != null && Guid.TryParse(claim, out var id) ? id : null;
    }
}
