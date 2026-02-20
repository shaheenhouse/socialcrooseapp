using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Marketplace.Database;
using Marketplace.Database.Entities;

namespace Marketplace.Api.Endpoints;

public static class WishlistEndpoints
{
    public static void MapWishlistEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/wishlist")
            .WithTags("Wishlist")
            .RequireAuthorization();

        group.MapGet("/", async (HttpContext context, MarketplaceDbContext db) =>
        {
            var userId = GetUserId(context);
            var wishlists = await db.Wishlists.AsNoTracking()
                .Where(w => w.UserId == userId)
                .Include(w => w.Items).ThenInclude(i => i.Product)
                .Include(w => w.Items).ThenInclude(i => i.Service)
                .OrderByDescending(w => w.CreatedAt)
                .Select(w => new
                {
                    w.Id, w.Name, w.Description, w.IsPublic, w.IsDefault, w.ItemCount, w.CreatedAt,
                    Items = w.Items.Select(i => new
                    {
                        i.Id, i.ProductId, i.ServiceId, i.ItemType, i.Priority, i.Notes,
                        i.PriceWhenAdded, i.NotifyOnPriceDrop, i.NotifyOnAvailability,
                        ProductName = i.Product != null ? i.Product.Name : null,
                        ServiceName = i.Service != null ? i.Service.Title : null
                    })
                })
                .ToListAsync();
            return Results.Ok(wishlists);
        }).WithName("GetWishlists").WithSummary("Get user's wishlists");

        group.MapPost("/", async ([FromBody] CreateWishlistRequest req, HttpContext context, MarketplaceDbContext db) =>
        {
            var userId = GetUserId(context);
            var wishlist = new Wishlist
            {
                UserId = userId, Name = req.Name,
                Description = req.Description, IsPublic = req.IsPublic
            };
            db.Wishlists.Add(wishlist);
            await db.SaveChangesAsync();
            return Results.Created($"/api/wishlist/{wishlist.Id}", new { wishlist.Id, wishlist.Name, wishlist.CreatedAt });
        }).WithName("CreateWishlist").WithSummary("Create a wishlist");

        group.MapPost("/{id:guid}/items", async (Guid id, [FromBody] AddWishlistItemRequest req, HttpContext context, MarketplaceDbContext db) =>
        {
            var userId = GetUserId(context);
            var wishlist = await db.Wishlists.Include(w => w.Items)
                .FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);
            if (wishlist == null) return Results.NotFound();

            var alreadyExists = wishlist.Items.Any(i => i.ProductId == req.ProductId && i.ServiceId == req.ServiceId);
            if (alreadyExists) return Results.Conflict(new { error = "Item already in wishlist" });

            decimal? priceWhenAdded = null;
            string itemType = "Product";
            if (req.ProductId.HasValue)
            {
                var product = await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == req.ProductId.Value);
                priceWhenAdded = product?.Price;
            }
            else if (req.ServiceId.HasValue)
            {
                var service = await db.Services.AsNoTracking().FirstOrDefaultAsync(s => s.Id == req.ServiceId.Value);
                priceWhenAdded = service?.BasePrice;
                itemType = "Service";
            }

            var item = new WishlistItem
            {
                WishlistId = id, ProductId = req.ProductId, ServiceId = req.ServiceId,
                ItemType = itemType, PriceWhenAdded = priceWhenAdded
            };
            wishlist.Items.Add(item);
            wishlist.ItemCount = wishlist.Items.Count;
            await db.SaveChangesAsync();
            return Results.Ok(new { item.Id, item.ProductId, item.ServiceId, item.ItemType, item.PriceWhenAdded });
        }).WithName("AddWishlistItem").WithSummary("Add item to wishlist");

        group.MapDelete("/{id:guid}/items/{itemId:guid}", async (Guid id, Guid itemId, HttpContext context, MarketplaceDbContext db) =>
        {
            var userId = GetUserId(context);
            var wishlist = await db.Wishlists.Include(w => w.Items)
                .FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);
            if (wishlist == null) return Results.NotFound();
            var item = wishlist.Items.FirstOrDefault(i => i.Id == itemId);
            if (item == null) return Results.NotFound();
            wishlist.Items.Remove(item);
            db.WishlistItems.Remove(item);
            wishlist.ItemCount = wishlist.Items.Count;
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).WithName("RemoveWishlistItem").WithSummary("Remove item from wishlist");

        group.MapDelete("/{id:guid}", async (Guid id, HttpContext context, MarketplaceDbContext db) =>
        {
            var userId = GetUserId(context);
            var wishlist = await db.Wishlists.FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);
            if (wishlist == null) return Results.NotFound();
            db.Wishlists.Remove(wishlist);
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).WithName("DeleteWishlist").WithSummary("Delete a wishlist");
    }

    private static Guid GetUserId(HttpContext context)
    {
        var claim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return claim != null ? Guid.Parse(claim) : throw new UnauthorizedAccessException();
    }
}

public record CreateWishlistRequest(string Name, string? Description = null, bool IsPublic = false);
public record AddWishlistItemRequest(Guid? ProductId = null, Guid? ServiceId = null);