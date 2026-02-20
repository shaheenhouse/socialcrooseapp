using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Marketplace.Database;
using Marketplace.Database.Entities;

namespace Marketplace.Api.Endpoints;

public static class DiscountEndpoints
{
    public static void MapDiscountEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/discounts")
            .WithTags("Discounts")
            .RequireAuthorization();

        group.MapGet("/", async (HttpContext context, MarketplaceDbContext db) =>
        {
            var userId = GetUserId(context);
            var storeIds = await db.Stores.AsNoTracking()
                .Where(s => s.OwnerId == userId).Select(s => s.Id).ToListAsync();
            var discounts = await db.Discounts.AsNoTracking()
                .Where(d => d.StoreId.HasValue && storeIds.Contains(d.StoreId.Value))
                .OrderByDescending(d => d.CreatedAt)
                .Select(d => new
                {
                    d.Id, d.Code, d.Name, d.Description, d.DiscountType, d.Value,
                    d.MinOrderAmount, d.MaxDiscountAmount, d.StartsAt, d.EndsAt,
                    d.UsageLimit, d.UsageCount, d.IsActive, d.StoreId, d.CreatedAt
                })
                .ToListAsync();
            return Results.Ok(discounts);
        }).WithName("GetDiscounts").WithSummary("Get discounts by store owner");

        group.MapGet("/{id:guid}", async (Guid id, HttpContext context, MarketplaceDbContext db) =>
        {
            var discount = await db.Discounts.AsNoTracking()
                .Where(d => d.Id == id)
                .Select(d => new
                {
                    d.Id, d.Code, d.Name, d.Description, d.DiscountType, d.Value,
                    d.MinOrderAmount, d.MaxDiscountAmount, d.StartsAt, d.EndsAt,
                    d.UsageLimit, d.UsageLimitPerUser, d.UsageCount, d.IsActive,
                    d.IsFirstOrderOnly, d.ApplicableProducts, d.ApplicableCategories,
                    d.StoreId, d.CreatedAt
                })
                .FirstOrDefaultAsync();
            return discount == null ? Results.NotFound() : Results.Ok(discount);
        }).WithName("GetDiscountById").WithSummary("Get discount by ID");

        group.MapPost("/", async ([FromBody] CreateDiscountRequest req, HttpContext context, MarketplaceDbContext db) =>
        {
            var userId = GetUserId(context);
            var existingCode = await db.Discounts.AsNoTracking().AnyAsync(d => d.Code == req.Code && d.StoreId == req.StoreId);
            if (existingCode) return Results.Conflict(new { error = "Discount code already exists for this store" });

            var discount = new Discount
            {
                Code = req.Code, Name = req.Name, Description = req.Description,
                DiscountType = req.DiscountType, Value = req.Value,
                MinOrderAmount = req.MinOrderAmount, MaxDiscountAmount = req.MaxDiscountAmount,
                StartsAt = req.StartsAt ?? DateTime.UtcNow, EndsAt = req.EndsAt,
                UsageLimit = req.UsageLimit, StoreId = req.StoreId, IsActive = true
            };
            discount.CreatedBy = userId;
            db.Discounts.Add(discount);
            await db.SaveChangesAsync();
            return Results.Created($"/api/discounts/{discount.Id}",
                new { discount.Id, discount.Code, discount.Name, discount.DiscountType, discount.Value, discount.CreatedAt });
        }).WithName("CreateDiscount").WithSummary("Create a discount");

        group.MapPatch("/{id:guid}", async (Guid id, [FromBody] UpdateDiscountRequest req, HttpContext context, MarketplaceDbContext db) =>
        {
            var userId = GetUserId(context);
            var discount = await db.Discounts.FirstOrDefaultAsync(d => d.Id == id);
            if (discount == null) return Results.NotFound();

            if (req.Name != null) discount.Name = req.Name;
            if (req.Description != null) discount.Description = req.Description;
            if (req.Value.HasValue) discount.Value = req.Value.Value;
            if (req.MinOrderAmount.HasValue) discount.MinOrderAmount = req.MinOrderAmount;
            if (req.MaxDiscountAmount.HasValue) discount.MaxDiscountAmount = req.MaxDiscountAmount;
            if (req.EndsAt.HasValue) discount.EndsAt = req.EndsAt;
            if (req.UsageLimit.HasValue) discount.UsageLimit = req.UsageLimit;
            if (req.IsActive.HasValue) discount.IsActive = req.IsActive.Value;
            discount.UpdatedBy = userId;

            await db.SaveChangesAsync();
            return Results.Ok(new { discount.Id, discount.Code, discount.Name, discount.IsActive, discount.UpdatedAt });
        }).WithName("UpdateDiscount").WithSummary("Update a discount");

        group.MapDelete("/{id:guid}", async (Guid id, HttpContext context, MarketplaceDbContext db) =>
        {
            var discount = await db.Discounts.FirstOrDefaultAsync(d => d.Id == id);
            if (discount == null) return Results.NotFound();
            db.Discounts.Remove(discount);
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).WithName("DeleteDiscount").WithSummary("Soft delete a discount");

        group.MapPost("/validate", async ([FromBody] ValidateDiscountRequest req, HttpContext context, MarketplaceDbContext db) =>
        {
            var userId = GetUserId(context);
            var discount = await db.Discounts.AsNoTracking()
                .FirstOrDefaultAsync(d => d.Code == req.Code && d.IsActive);

            if (discount == null)
                return Results.BadRequest(new { valid = false, error = "Invalid discount code" });
            if (discount.EndsAt.HasValue && discount.EndsAt.Value < DateTime.UtcNow)
                return Results.BadRequest(new { valid = false, error = "Discount code has expired" });
            if (discount.StartsAt > DateTime.UtcNow)
                return Results.BadRequest(new { valid = false, error = "Discount code is not yet active" });
            if (discount.UsageLimit.HasValue && discount.UsageCount >= discount.UsageLimit.Value)
                return Results.BadRequest(new { valid = false, error = "Discount usage limit reached" });
            if (discount.MinOrderAmount.HasValue && req.OrderAmount < discount.MinOrderAmount.Value)
                return Results.BadRequest(new { valid = false, error = $"Minimum order amount is {discount.MinOrderAmount.Value}" });

            if (discount.UsageLimitPerUser.HasValue)
            {
                var userUsageCount = await db.DiscountUsages.AsNoTracking()
                    .CountAsync(u => u.DiscountId == discount.Id && u.UserId == userId);
                if (userUsageCount >= discount.UsageLimitPerUser.Value)
                    return Results.BadRequest(new { valid = false, error = "You have reached the usage limit for this discount" });
            }

            decimal discountAmount = discount.DiscountType == "Percentage"
                ? req.OrderAmount * discount.Value / 100
                : discount.Value;
            if (discount.MaxDiscountAmount.HasValue && discountAmount > discount.MaxDiscountAmount.Value)
                discountAmount = discount.MaxDiscountAmount.Value;

            return Results.Ok(new
            {
                valid = true, discount.Code, discount.DiscountType, discount.Value,
                discountAmount, finalAmount = req.OrderAmount - discountAmount
            });
        }).WithName("ValidateDiscount").WithSummary("Validate a discount code");
    }

    private static Guid GetUserId(HttpContext context)
    {
        var claim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return claim != null ? Guid.Parse(claim) : throw new UnauthorizedAccessException();
    }
}

public record CreateDiscountRequest(string Code, string Name, string? Description,
    string DiscountType, decimal Value, decimal? MinOrderAmount = null,
    decimal? MaxDiscountAmount = null, int? UsageLimit = null,
    DateTime? StartsAt = null, DateTime? EndsAt = null, Guid? StoreId = null);
public record UpdateDiscountRequest(string? Name = null, string? Description = null,
    decimal? Value = null, decimal? MinOrderAmount = null,
    decimal? MaxDiscountAmount = null, int? UsageLimit = null,
    DateTime? EndsAt = null, bool? IsActive = null);
public record ValidateDiscountRequest(string Code, decimal OrderAmount);