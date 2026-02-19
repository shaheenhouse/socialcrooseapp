using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Marketplace.Slices.OrderSlice;
using Marketplace.Database.Enums;

namespace Marketplace.Api.Endpoints;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders").WithTags("Orders");

        group.MapGet("/", async (HttpContext context,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? status = null,
            IOrderService orderService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var (orders, totalCount) = await orderService.GetMyOrdersAsync(userId.Value, page, pageSize, status);
            return Results.Ok(new
            {
                data = orders,
                pagination = new { page, pageSize, totalCount, totalPages = (int)Math.Ceiling(totalCount / (double)pageSize) }
            });
        })
        .RequireAuthorization()
        .WithName("GetUserOrders");

        group.MapGet("/sales", async (HttpContext context,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? status = null,
            IOrderService orderService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var (orders, totalCount) = await orderService.GetMySalesAsync(userId.Value, page, pageSize, status);
            return Results.Ok(new
            {
                data = orders,
                pagination = new { page, pageSize, totalCount, totalPages = (int)Math.Ceiling(totalCount / (double)pageSize) }
            });
        })
        .RequireAuthorization()
        .WithName("GetUserSales");

        group.MapGet("/{id:guid}", async (HttpContext context, Guid id, IOrderService orderService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var order = await orderService.GetByIdAsync(id, userId.Value);
            return order != null ? Results.Ok(order) : Results.NotFound();
        })
        .RequireAuthorization()
        .WithName("GetOrderById");

        group.MapGet("/{id:guid}/items", async (HttpContext context, Guid id, IOrderService orderService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var items = await orderService.GetItemsAsync(id, userId.Value);
            return Results.Ok(new { data = items });
        })
        .RequireAuthorization()
        .WithName("GetOrderItems");

        group.MapPost("/", async (HttpContext context, [FromBody] CreateOrderDto dto, IOrderService orderService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var id = await orderService.CreateAsync(dto, userId.Value);
            return Results.Created($"/api/orders/{id}", new { id });
        })
        .RequireAuthorization()
        .WithName("CreateOrder");

        group.MapPatch("/{id:guid}/status", async (HttpContext context, Guid id,
            [FromBody] UpdateOrderStatusRequest request, IOrderService orderService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            if (!Enum.TryParse<OrderStatus>(request.Status, true, out var status))
                return Results.BadRequest(new { error = "Invalid status" });
            var result = await orderService.UpdateStatusAsync(id, userId.Value, status, request.Notes);
            return result ? Results.Ok(new { message = "Status updated" }) : Results.NotFound();
        })
        .RequireAuthorization()
        .WithName("UpdateOrderStatus");

        group.MapPost("/{id:guid}/cancel", async (HttpContext context, Guid id,
            [FromBody] CancelOrderRequest? request, IOrderService orderService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var result = await orderService.CancelAsync(id, userId.Value, request?.Reason);
            return result ? Results.Ok(new { message = "Order cancelled" }) : Results.BadRequest(new { error = "Cannot cancel this order" });
        })
        .RequireAuthorization()
        .WithName("CancelOrder");
    }

    private static Guid? GetUserId(HttpContext context)
    {
        var claim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return claim != null && Guid.TryParse(claim, out var id) ? id : null;
    }
}

public record UpdateOrderStatusRequest(string Status, string? Notes = null);
public record CancelOrderRequest(string? Reason = null);
