using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Marketplace.Database;
using Marketplace.Database.Entities;

namespace Marketplace.Api.Endpoints;

public static class InvoiceEndpoints
{
    public static void MapInvoiceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/invoices")
            .WithTags("Invoices")
            .RequireAuthorization();

        group.MapGet("/", async (
            HttpContext context, MarketplaceDbContext db,
            [FromQuery] string? status,
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20) =>
        {
            var userId = GetUserId(context);
            var skip = (page - 1) * pageSize;
            var query = db.Invoices.AsNoTracking().Where(i => i.UserId == userId);
            if (!string.IsNullOrEmpty(status))
                query = query.Where(i => i.Status == status);

            var invoices = await query
                .OrderByDescending(i => i.CreatedAt)
                .Skip(skip).Take(pageSize)
                .Select(i => new
                {
                    i.Id, i.InvoiceNumber, i.ClientName, i.ClientEmail,
                    i.IssueDate, i.DueDate, i.Status, i.Subtotal,
                    i.TaxAmount, i.DiscountAmount, i.Total, i.Currency,
                    i.PaidAt, i.CreatedAt,
                    ItemCount = i.Items.Count
                })
                .ToListAsync();
            var total = await query.CountAsync();
            return Results.Ok(new { items = invoices, total, page, pageSize });
        }).WithName("GetInvoices").WithSummary("List invoices paginated with optional status filter");

        group.MapGet("/{id:guid}", async (Guid id, HttpContext context, MarketplaceDbContext db) =>
        {
            var userId = GetUserId(context);
            var invoice = await db.Invoices.AsNoTracking()
                .Include(i => i.Items)
                .Where(i => i.Id == id && i.UserId == userId)
                .Select(i => new
                {
                    i.Id, i.InvoiceNumber, i.ClientName, i.ClientEmail,
                    i.ClientPhone, i.ClientAddress, i.IssueDate, i.DueDate,
                    i.Status, i.Subtotal, i.TaxRate, i.TaxAmount,
                    i.DiscountAmount, i.Total, i.Currency, i.Notes, i.Terms,
                    i.PaidAt, i.PaymentMethod, i.CreatedAt, i.UpdatedAt,
                    Items = i.Items.Select(item => new
                    {
                        item.Id, item.Description, item.Quantity,
                        item.Unit, item.UnitPrice, item.Total
                    })
                })
                .FirstOrDefaultAsync();
            return invoice == null ? Results.NotFound() : Results.Ok(invoice);
        }).WithName("GetInvoiceById").WithSummary("Get invoice by ID");

        group.MapPost("/", async ([FromBody] CreateInvoiceRequest req, HttpContext context, MarketplaceDbContext db) =>
        {
            var userId = GetUserId(context);
            var lastInvoice = await db.Invoices.AsNoTracking()
                .Where(i => i.UserId == userId)
                .OrderByDescending(i => i.CreatedAt)
                .Select(i => i.InvoiceNumber)
                .FirstOrDefaultAsync();

            var nextNumber = 1;
            if (lastInvoice != null && lastInvoice.StartsWith("INV-"))
            {
                var parts = lastInvoice.Split('-');
                if (parts.Length >= 2 && int.TryParse(parts[^1], out var num))
                    nextNumber = num + 1;
            }

            var invoice = new Invoice
            {
                UserId = userId,
                InvoiceNumber = $"INV-{nextNumber:D5}",
                ClientName = req.ClientName, ClientEmail = req.ClientEmail,
                ClientPhone = req.ClientPhone, ClientAddress = req.ClientAddress,
                IssueDate = req.IssueDate ?? DateTime.UtcNow,
                DueDate = req.DueDate,
                Status = "draft", Currency = req.Currency ?? "PKR",
                Notes = req.Notes, Terms = req.Terms,
                TaxRate = req.TaxRate,
                PublicToken = Guid.NewGuid().ToString("N")
            };

            decimal subtotal = 0;
            foreach (var line in req.Items)
            {
                var lineTotal = line.Quantity * line.UnitPrice;
                subtotal += lineTotal;
                invoice.Items.Add(new InvoiceItem
                {
                    Description = line.Description,
                    Quantity = line.Quantity,
                    Unit = line.Unit ?? "unit",
                    UnitPrice = line.UnitPrice,
                    Total = lineTotal
                });
            }

            invoice.Subtotal = subtotal;
            invoice.TaxAmount = subtotal * (req.TaxRate / 100m);
            invoice.DiscountAmount = req.DiscountAmount;
            invoice.Total = invoice.Subtotal + invoice.TaxAmount - req.DiscountAmount;
            invoice.CreatedBy = userId;

            db.Invoices.Add(invoice);
            await db.SaveChangesAsync();
            return Results.Created($"/api/invoices/{invoice.Id}",
                new { invoice.Id, invoice.InvoiceNumber, invoice.ClientName, invoice.Total, invoice.Status, invoice.CreatedAt });
        }).WithName("CreateInvoice").WithSummary("Create an invoice with items");

        group.MapPatch("/{id:guid}", async (Guid id, [FromBody] UpdateInvoiceRequest req, HttpContext context, MarketplaceDbContext db) =>
        {
            var userId = GetUserId(context);
            var invoice = await db.Invoices.FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);
            if (invoice == null) return Results.NotFound();

            if (req.ClientName != null) invoice.ClientName = req.ClientName;
            if (req.ClientEmail != null) invoice.ClientEmail = req.ClientEmail;
            if (req.ClientPhone != null) invoice.ClientPhone = req.ClientPhone;
            if (req.ClientAddress != null) invoice.ClientAddress = req.ClientAddress;
            if (req.DueDate.HasValue) invoice.DueDate = req.DueDate.Value;
            if (req.Notes != null) invoice.Notes = req.Notes;
            if (req.Terms != null) invoice.Terms = req.Terms;
            invoice.UpdatedBy = userId;

            await db.SaveChangesAsync();
            return Results.Ok(new { invoice.Id, invoice.InvoiceNumber, invoice.ClientName, invoice.UpdatedAt });
        }).WithName("UpdateInvoice").WithSummary("Update an invoice");

        group.MapDelete("/{id:guid}", async (Guid id, HttpContext context, MarketplaceDbContext db) =>
        {
            var userId = GetUserId(context);
            var invoice = await db.Invoices.FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);
            if (invoice == null) return Results.NotFound();
            db.Invoices.Remove(invoice);
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).WithName("DeleteInvoice").WithSummary("Soft delete an invoice");

        group.MapPost("/{id:guid}/mark-paid", async (Guid id, [FromBody] MarkPaidRequest? req, HttpContext context, MarketplaceDbContext db) =>
        {
            var userId = GetUserId(context);
            var invoice = await db.Invoices.FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);
            if (invoice == null) return Results.NotFound();
            if (invoice.Status == "paid")
                return Results.BadRequest(new { error = "Invoice is already paid" });

            invoice.Status = "paid";
            invoice.PaidAt = DateTime.UtcNow;
            invoice.PaymentMethod = req?.PaymentMethod;
            invoice.UpdatedBy = userId;

            await db.SaveChangesAsync();
            return Results.Ok(new { invoice.Id, invoice.InvoiceNumber, invoice.Status, invoice.PaidAt, invoice.PaymentMethod });
        }).WithName("MarkInvoicePaid").WithSummary("Mark an invoice as paid");

        group.MapPost("/{id:guid}/send", async (Guid id, HttpContext context, MarketplaceDbContext db) =>
        {
            var userId = GetUserId(context);
            var invoice = await db.Invoices.FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);
            if (invoice == null) return Results.NotFound();
            if (invoice.Status != "draft")
                return Results.BadRequest(new { error = "Only draft invoices can be sent" });

            invoice.Status = "sent";
            invoice.UpdatedBy = userId;

            await db.SaveChangesAsync();
            return Results.Ok(new { invoice.Id, invoice.InvoiceNumber, invoice.Status, invoice.PublicToken });
        }).WithName("SendInvoice").WithSummary("Mark an invoice as sent");
    }

    private static Guid GetUserId(HttpContext context)
    {
        var claim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return claim != null ? Guid.Parse(claim) : throw new UnauthorizedAccessException();
    }
}

public record CreateInvoiceRequest(string ClientName, string? ClientEmail,
    string? ClientPhone, string? ClientAddress, DateTime? IssueDate,
    DateTime DueDate, string? Currency, decimal TaxRate, decimal DiscountAmount,
    string? Notes, string? Terms, List<CreateInvoiceItemRequest> Items);
public record CreateInvoiceItemRequest(string Description, decimal Quantity,
    string? Unit, decimal UnitPrice);
public record UpdateInvoiceRequest(string? ClientName = null, string? ClientEmail = null,
    string? ClientPhone = null, string? ClientAddress = null,
    DateTime? DueDate = null, string? Notes = null, string? Terms = null);
public record MarkPaidRequest(string? PaymentMethod = null);