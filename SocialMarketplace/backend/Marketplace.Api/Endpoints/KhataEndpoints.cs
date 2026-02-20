using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Marketplace.Slices.KhataSlice;

namespace Marketplace.Api.Endpoints;

public static class KhataEndpoints
{
    public static void MapKhataEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/khata")
            .WithTags("Khata / Ledger")
            .RequireAuthorization();

        group.MapGet("/", async (
            HttpContext context,
            IKhataService khataService,
            [FromQuery] string? search,
            [FromQuery] string? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20) =>
        {
            var userId = GetUserId(context);
            var result = await khataService.GetPartiesAsync(userId, new KhataQueryParams(search, status, page, pageSize));
            return Results.Ok(result);
        })
        .WithName("GetKhataParties")
        .WithSummary("Get all khata parties for the current user");

        group.MapGet("/{id:guid}", async (Guid id, HttpContext context, IKhataService khataService) =>
        {
            var userId = GetUserId(context);
            var party = await khataService.GetPartyByIdAsync(userId, id);
            return party == null ? Results.NotFound() : Results.Ok(party);
        })
        .WithName("GetKhataParty")
        .WithSummary("Get a specific khata party");

        group.MapPost("/", async ([FromBody] CreateKhataPartyDto dto, HttpContext context, IKhataService khataService) =>
        {
            var userId = GetUserId(context);
            var result = await khataService.CreatePartyAsync(userId, dto);
            return Results.Created($"/api/khata/{result.Id}", result);
        })
        .WithName("CreateKhataParty")
        .WithSummary("Create a new khata party (customer or supplier)");

        group.MapPatch("/{id:guid}", async (Guid id, [FromBody] UpdateKhataPartyDto dto, HttpContext context, IKhataService khataService) =>
        {
            var userId = GetUserId(context);
            var success = await khataService.UpdatePartyAsync(userId, id, dto);
            return success ? Results.Ok() : Results.NotFound();
        })
        .WithName("UpdateKhataParty")
        .WithSummary("Update a khata party");

        group.MapDelete("/{id:guid}", async (Guid id, HttpContext context, IKhataService khataService) =>
        {
            var userId = GetUserId(context);
            var success = await khataService.DeletePartyAsync(userId, id);
            return success ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteKhataParty")
        .WithSummary("Delete a khata party");

        group.MapGet("/{id:guid}/entries", async (
            Guid id,
            HttpContext context,
            IKhataService khataService,
            [FromQuery] string? type,
            [FromQuery] string? from,
            [FromQuery] string? to,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50) =>
        {
            var userId = GetUserId(context);
            var result = await khataService.GetEntriesAsync(userId, id, new KhataEntryQueryParams(type, from, to, page, pageSize));
            return Results.Ok(result);
        })
        .WithName("GetKhataEntries")
        .WithSummary("Get entries (transactions) for a khata party");

        group.MapPost("/{id:guid}/entries", async (Guid id, [FromBody] CreateKhataEntryDto dto, HttpContext context, IKhataService khataService) =>
        {
            var userId = GetUserId(context);
            var result = await khataService.AddEntryAsync(userId, id, dto);
            return Results.Created($"/api/khata/{id}/entries/{result.Id}", result);
        })
        .WithName("AddKhataEntry")
        .WithSummary("Add a credit or debit entry to a khata party");

        group.MapGet("/summary", async (HttpContext context, IKhataService khataService, [FromQuery] string? from, [FromQuery] string? to) =>
        {
            var userId = GetUserId(context);
            var range = (from != null || to != null) ? new DateRangeParams(from, to) : null;
            var result = await khataService.GetSummaryAsync(userId, range);
            return Results.Ok(result);
        })
        .WithName("GetKhataSummary")
        .WithSummary("Get khata summary (receivable, payable, net balance)");

        group.MapPost("/{id:guid}/reminder", async (Guid id, HttpContext context, IKhataService khataService) =>
        {
            var userId = GetUserId(context);
            var success = await khataService.SendReminderAsync(userId, id);
            return success ? Results.Ok(new { message = "Reminder sent" }) : Results.NotFound();
        })
        .WithName("SendKhataReminder")
        .WithSummary("Send a payment reminder to a khata party");
    }

    private static Guid GetUserId(HttpContext context)
    {
        var claim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return claim != null ? Guid.Parse(claim) : throw new UnauthorizedAccessException();
    }
}
