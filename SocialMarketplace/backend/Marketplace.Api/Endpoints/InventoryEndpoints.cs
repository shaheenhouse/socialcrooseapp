using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Marketplace.Database;
using Marketplace.Database.Entities;

namespace Marketplace.Api.Endpoints;

public static class InventoryEndpoints
{
    public static void MapInventoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/inventory")
            .WithTags("Inventory")
            .RequireAuthorization();

        group.MapGet("/", async (
            HttpContext context, MarketplaceDbContext db,
            [FromQuery] string? search, [FromQuery] bool? lowStock,
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20) =>
        {
            var userId = GetUserId(context);
            var skip = (page - 1) * pageSize;
            var query = db.InventoryItems.AsNoTracking().Where(i => i.UserId == userId);
            if (!string.IsNullOrEmpty(search))
                query = query.Where(i => i.Name.Contains(search) || (i.Sku != null && i.Sku.Contains(search)));
            if (lowStock == true)
                query = query.Where(i => i.Quantity <= i.ReorderLevel);

            var items = await query
                .OrderByDescending(i => i.CreatedAt)
                .Skip(skip).Take(pageSize)
                .Select(i => new
                {
                    i.Id, i.Name, i.Sku, i.Barcode, i.Description, i.Category,
                    i.Quantity, i.ReorderLevel, i.Unit, i.CostPrice, i.UnitPrice,
                    i.ImageUrl, i.Location, i.TrackInventory, i.StoreId, i.CreatedAt,
                    IsLowStock = i.Quantity <= i.ReorderLevel
                })
                .ToListAsync();
            var total = await query.CountAsync();
            return Results.Ok(new { items, total, page, pageSize });
        }).WithName("GetInventoryItems").WithSummary("List inventory items paginated");

        group.MapGet("/low-stock", async (HttpContext context, MarketplaceDbContext db) =>
        {
            var userId = GetUserId(context);
            var items = await db.InventoryItems.AsNoTracking()
                .Where(i => i.UserId == userId && i.Quantity <= i.ReorderLevel)
                .OrderBy(i => i.Quantity)
                .Select(i => new
                {
                    i.Id, i.Name, i.Sku, i.Quantity, i.ReorderLevel,
                    i.UnitPrice, i.StoreId, Deficit = i.ReorderLevel - i.Quantity
                })
                .ToListAsync();
            return Results.Ok(items);
        }).WithName("GetLowStockItems").WithSummary("Get items where quantity is at or below reorder level");

        group.MapGet("/{id:guid}", async (Guid id, HttpContext context, MarketplaceDbContext db) =>
        {
            var userId = GetUserId(context);
            var item = await db.InventoryItems.AsNoTracking()
                .Where(i => i.Id == id && i.UserId == userId)
                .Select(i => new
                {
                    i.Id, i.Name, i.Sku, i.Barcode, i.Description, i.Category,
                    i.Quantity, i.ReorderLevel, i.Unit, i.CostPrice, i.UnitPrice,
                    i.ImageUrl, i.Location, i.TrackInventory, i.StoreId,
                    i.CreatedAt, i.UpdatedAt,
                    IsLowStock = i.Quantity <= i.ReorderLevel
                })
                .FirstOrDefaultAsync();
            return item == null ? Results.NotFound() : Results.Ok(item);
        }).WithName("GetInventoryItemById").WithSummary("Get inventory item by ID");

        group.MapPost("/", async ([FromBody] CreateInventoryItemRequest req, HttpContext context, MarketplaceDbContext db) =>
        {
            var userId = GetUserId(context);
            var item = new InventoryItem
            {
                UserId = userId, StoreId = req.StoreId, Name = req.Name,
                Sku = req.Sku, Barcode = req.Barcode, Description = req.Description,
                Category = req.Category, Quantity = req.Quantity,
                ReorderLevel = req.ReorderLevel, Unit = req.Unit ?? "unit",
                CostPrice = req.CostPrice, UnitPrice = req.UnitPrice,
                ImageUrl = req.ImageUrl, Location = req.Location
            };
            item.CreatedBy = userId;
            db.InventoryItems.Add(item);

            if (req.Quantity > 0)
            {
                db.InventoryMovements.Add(new InventoryMovement
                {
                    InventoryItemId = item.Id,
                    QuantityChange = req.Quantity,
                    QuantityBefore = 0, QuantityAfter = req.Quantity,
                    Type = "purchase", Reason = "Initial stock"
                });
            }

            await db.SaveChangesAsync();
            return Results.Created($"/api/inventory/{item.Id}",
                new { item.Id, item.Name, item.Sku, item.Quantity, item.UnitPrice, item.CreatedAt });
        }).WithName("CreateInventoryItem").WithSummary("Create an inventory item");

        group.MapPatch("/{id:guid}", async (Guid id, [FromBody] UpdateInventoryItemRequest req, HttpContext context, MarketplaceDbContext db) =>
        {
            var userId = GetUserId(context);
            var item = await db.InventoryItems.FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);
            if (item == null) return Results.NotFound();

            if (req.Name != null) item.Name = req.Name;
            if (req.Sku != null) item.Sku = req.Sku;
            if (req.Description != null) item.Description = req.Description;
            if (req.Category != null) item.Category = req.Category;
            if (req.ReorderLevel.HasValue) item.ReorderLevel = req.ReorderLevel.Value;
            if (req.Unit != null) item.Unit = req.Unit;
            if (req.CostPrice.HasValue) item.CostPrice = req.CostPrice.Value;
            if (req.UnitPrice.HasValue) item.UnitPrice = req.UnitPrice.Value;
            if (req.ImageUrl != null) item.ImageUrl = req.ImageUrl;
            if (req.Location != null) item.Location = req.Location;
            item.UpdatedBy = userId;

            await db.SaveChangesAsync();
            return Results.Ok(new { item.Id, item.Name, item.Sku, item.Quantity, item.UpdatedAt });
        }).WithName("UpdateInventoryItem").WithSummary("Update an inventory item");

        group.MapDelete("/{id:guid}", async (Guid id, HttpContext context, MarketplaceDbContext db) =>
        {
            var userId = GetUserId(context);
            var item = await db.InventoryItems.FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);
            if (item == null) return Results.NotFound();
            db.InventoryItems.Remove(item);
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).WithName("DeleteInventoryItem").WithSummary("Soft delete an inventory item");

        group.MapPost("/{id:guid}/adjust", async (Guid id, [FromBody] AdjustStockRequest req, HttpContext context, MarketplaceDbContext db) =>
        {
            var userId = GetUserId(context);
            var item = await db.InventoryItems.FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);
            if (item == null) return Results.NotFound();

            var before = item.Quantity;
            item.Quantity += req.QuantityChange;
            if (item.Quantity < 0) return Results.BadRequest(new { error = "Stock cannot go below zero" });

            var movement = new InventoryMovement
            {
                InventoryItemId = id, QuantityChange = req.QuantityChange,
                QuantityBefore = before, QuantityAfter = item.Quantity,
                Type = req.Type ?? "adjustment", Reason = req.Reason ?? string.Empty,
                ReferenceId = req.ReferenceId
            };
            movement.CreatedBy = userId;
            db.InventoryMovements.Add(movement);

            await db.SaveChangesAsync();
            return Results.Ok(new { item.Id, item.Quantity, movement = new { movement.Id, movement.QuantityChange, movement.Type, movement.Reason } });
        }).WithName("AdjustStock").WithSummary("Adjust stock quantity");

        group.MapGet("/{id:guid}/movements", async (
            Guid id, HttpContext context, MarketplaceDbContext db,
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20) =>
        {
            var userId = GetUserId(context);
            var itemExists = await db.InventoryItems.AsNoTracking().AnyAsync(i => i.Id == id && i.UserId == userId);
            if (!itemExists) return Results.NotFound();

            var skip = (page - 1) * pageSize;
            var movements = await db.InventoryMovements.AsNoTracking()
                .Where(m => m.InventoryItemId == id)
                .OrderByDescending(m => m.CreatedAt)
                .Skip(skip).Take(pageSize)
                .Select(m => new
                {
                    m.Id, m.QuantityChange, m.QuantityBefore, m.QuantityAfter,
                    m.Type, m.Reason, m.ReferenceId, m.CreatedAt
                })
                .ToListAsync();
            var total = await db.InventoryMovements.AsNoTracking().CountAsync(m => m.InventoryItemId == id);
            return Results.Ok(new { items = movements, total, page, pageSize });
        }).WithName("GetInventoryMovements").WithSummary("Get stock movements for an item");
    }

    private static Guid GetUserId(HttpContext context)
    {
        var claim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return claim != null ? Guid.Parse(claim) : throw new UnauthorizedAccessException();
    }
}

public record CreateInventoryItemRequest(string Name, string? Sku, string? Barcode,
    string? Description, string? Category, int Quantity, int ReorderLevel,
    string? Unit, decimal CostPrice, decimal UnitPrice,
    string? ImageUrl = null, string? Location = null, Guid? StoreId = null);
public record UpdateInventoryItemRequest(string? Name = null, string? Sku = null,
    string? Description = null, string? Category = null, int? ReorderLevel = null,
    string? Unit = null, decimal? CostPrice = null, decimal? UnitPrice = null,
    string? ImageUrl = null, string? Location = null);
public record AdjustStockRequest(int QuantityChange, string? Type = null,
    string? Reason = null, string? ReferenceId = null);