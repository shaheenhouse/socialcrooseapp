using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Marketplace.Slices.ProductSlice;

namespace Marketplace.Api.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products").WithTags("Products");

        group.MapGet("/", async (
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] Guid? category = null,
            [FromQuery] Guid? store = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] string? sortBy = null,
            IProductService productService) =>
        {
            var query = new ProductQueryParams
            {
                Page = page, PageSize = pageSize, Search = search,
                CategoryId = category, StoreId = store,
                MinPrice = minPrice, MaxPrice = maxPrice, SortBy = sortBy
            };
            var (products, totalCount) = await productService.GetAllAsync(query);
            return Results.Ok(new
            {
                data = products,
                pagination = new { page, pageSize, totalCount, totalPages = (int)Math.Ceiling(totalCount / (double)pageSize) }
            });
        })
        .WithName("GetAllProducts");

        group.MapGet("/{id:guid}", async (Guid id, IProductService productService) =>
        {
            var product = await productService.GetByIdAsync(id);
            return product != null ? Results.Ok(product) : Results.NotFound();
        })
        .WithName("GetProductById");

        group.MapGet("/slug/{slug}", async (string slug, IProductService productService) =>
        {
            var product = await productService.GetBySlugAsync(slug);
            return product != null ? Results.Ok(product) : Results.NotFound();
        })
        .WithName("GetProductBySlug");

        group.MapPost("/", async (HttpContext context, [FromBody] CreateProductRequest request, IProductService productService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var id = await productService.CreateAsync(request.Product, request.StoreId);
            return Results.Created($"/api/products/{id}", new { id });
        })
        .RequireAuthorization()
        .WithName("CreateProduct");

        group.MapPatch("/{id:guid}", async (HttpContext context, Guid id, [FromBody] UpdateProductDto dto, IProductService productService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var result = await productService.UpdateAsync(id, dto);
            return result ? Results.Ok(new { message = "Product updated" }) : Results.NotFound();
        })
        .RequireAuthorization()
        .WithName("UpdateProduct");

        group.MapDelete("/{id:guid}", async (HttpContext context, Guid id, IProductService productService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var result = await productService.DeleteAsync(id);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .RequireAuthorization()
        .WithName("DeleteProduct");

        group.MapGet("/{id:guid}/images", async (Guid id, IProductService productService) =>
        {
            var images = await productService.GetImagesAsync(id);
            return Results.Ok(new { data = images });
        })
        .WithName("GetProductImages");

        group.MapGet("/{id:guid}/variants", async (Guid id, IProductService productService) =>
        {
            var variants = await productService.GetVariantsAsync(id);
            return Results.Ok(new { data = variants });
        })
        .WithName("GetProductVariants");

        group.MapGet("/{id:guid}/reviews", async (Guid id,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            IProductService productService) =>
        {
            var reviews = await productService.GetReviewsAsync(id, page, pageSize);
            return Results.Ok(new { data = reviews });
        })
        .WithName("GetProductReviews");
    }

    private static Guid? GetUserId(HttpContext context)
    {
        var claim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return claim != null && Guid.TryParse(claim, out var id) ? id : null;
    }
}

public record CreateProductRequest(Guid StoreId, CreateProductDto Product);
