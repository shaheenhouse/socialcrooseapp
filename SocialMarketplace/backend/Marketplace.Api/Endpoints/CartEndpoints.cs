using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Marketplace.Database;
using Marketplace.Database.Entities;

namespace Marketplace.Api.Endpoints;

public static class CartEndpoints
{
    public static void MapCartEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/cart")
            .WithTags("Cart")
            .RequireAuthorization();

        group.MapGet("/", async (HttpContext context, MarketplaceDbContext db) =>
        {
            var userId = GetUserId(context);
            var cart = await db.Carts.AsNoTracking()
                .Include(c => c.Items).ThenInclude(i => i.Product)
                .Include(c => c.Items).ThenInclude(i => i.Service)
                .FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null)
                return Results.Ok(new { id = (Guid?)null, items = Array.Empty<object>(), subtotal = 0m, totalAmount = 0m });
            return Results.Ok(new
            {
                cart.Id, cart.UserId, cart.Subtotal, cart.TaxAmount,
                cart.ShippingAmount, cart.DiscountAmount, cart.DiscountCode,
                cart.TotalAmount, cart.Currency, cart.ItemCount,
                Items = cart.Items.Select(i => new
                {
                    i.Id, i.ProductId, i.ServiceId, i.ItemType,
                    i.Quantity, i.UnitPrice, i.TotalPrice, i.Notes,
                    ProductName = i.Product != null ? i.Product.Name : null,
                    ServiceName = i.Service != null ? i.Service.Title : null
                })
            });
        }).WithName("GetCart").WithSummary("Get current user's cart");

        group.MapPost("/items", async ([FromBody] AddCartItemRequest req, HttpContext context, MarketplaceDbContext db) =>
        {
            var userId = GetUserId(context);
            var cart = await db.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                db.Carts.Add(cart);
            }
            var existing = cart.Items.FirstOrDefault(i => i.ProductId == req.ProductId && i.ServiceId == req.ServiceId);
            if (existing != null)
            {
                existing.Quantity += req.Quantity;
                existing.TotalPrice = existing.UnitPrice * existing.Quantity;
            }
            else
            {
                decimal unitPrice = 0;
                string itemType = "Product";
                if (req.ProductId.HasValue)
                {
                    var product = await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == req.ProductId.Value);
                    if (product == null) return Results.BadRequest(new { error = "Product not found" });
                    unitPrice = product.Price;
                }
                else if (req.ServiceId.HasValue)
                {
                    var service = await db.Services.AsNoTracking().FirstOrDefaultAsync(s => s.Id == req.ServiceId.Value);
                    if (service == null) return Results.BadRequest(new { error = "Service not found" });
                    unitPrice = service.BasePrice;
                    itemType = "Service";
                }
                cart.Items.Add(new CartItem
                {
                    ProductId = req.ProductId, ServiceId = req.ServiceId,
                    ItemType = itemType, Quantity = req.Quantity,
                    UnitPrice = unitPrice, TotalPrice = unitPrice * req.Quantity
                });
            }
            RecalculateCart(cart);
            await db.SaveChangesAsync();
            return Results.Ok(new { cart.Id, cart.ItemCount, cart.TotalAmount });
        }).WithName("AddCartItem").WithSummary("Add item to cart");

        group.MapPatch("/items/{itemId:guid}", async (Guid itemId, [FromBody] UpdateCartItemRequest req, HttpContext context, MarketplaceDbContext db) =>
        {
            var userId = GetUserId(context);
            var cart = await db.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null) return Results.NotFound();
            var item = cart.Items.FirstOrDefault(i => i.Id == itemId);
            if (item == null) return Results.NotFound();
            item.Quantity = req.Quantity;
            item.TotalPrice = item.UnitPrice * item.Quantity;
            RecalculateCart(cart);
            await db.SaveChangesAsync();
            return Results.Ok(new { item.Id, item.Quantity, item.TotalPrice, cart.TotalAmount });
        }).WithName("UpdateCartItem").WithSummary("Update cart item quantity");

        group.MapDelete("/items/{itemId:guid}", async (Guid itemId, HttpContext context, MarketplaceDbContext db) =>
        {
            var userId = GetUserId(context);
            var cart = await db.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null) return Results.NotFound();
            var item = cart.Items.FirstOrDefault(i => i.Id == itemId);
            if (item == null) return Results.NotFound();
            cart.Items.Remove(item);
            db.CartItems.Remove(item);
            RecalculateCart(cart);
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).WithName("RemoveCartItem").WithSummary("Remove item from cart");

        group.MapDelete("/", async (HttpContext context, MarketplaceDbContext db) =>
        {
            var userId = GetUserId(context);
            var cart = await db.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null) return Results.NoContent();
            db.CartItems.RemoveRange(cart.Items);
            cart.Subtotal = 0; cart.TotalAmount = 0; cart.ItemCount = 0;
            cart.TaxAmount = 0; cart.ShippingAmount = 0;
            cart.DiscountAmount = null; cart.DiscountCode = null;
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).WithName("ClearCart").WithSummary("Clear all items from cart");
    }

    private static void RecalculateCart(Cart cart)
    {
        cart.Subtotal = cart.Items.Sum(i => i.TotalPrice);
        cart.ItemCount = cart.Items.Sum(i => i.Quantity);
        cart.TotalAmount = cart.Subtotal + cart.TaxAmount + cart.ShippingAmount - (cart.DiscountAmount ?? 0);
    }

    private static Guid GetUserId(HttpContext context)
    {
        var claim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return claim != null ? Guid.Parse(claim) : throw new UnauthorizedAccessException();
    }
}

public record AddCartItemRequest(Guid? ProductId, Guid? ServiceId = null, int Quantity = 1);
public record UpdateCartItemRequest(int Quantity);