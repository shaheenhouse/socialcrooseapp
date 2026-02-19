namespace Marketplace.Api.Endpoints;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders").WithTags("Orders");

        // Get user's orders
        group.MapGet("/", () =>
        {
            // TODO: Implement order listing
            return Results.Ok(new { data = Array.Empty<object>(), pagination = new { page = 1, pageSize = 20, totalCount = 0 } });
        })
        .RequireAuthorization()
        .WithName("GetUserOrders")
        .WithSummary("Get current user's orders");

        // Get order by ID
        group.MapGet("/{id:guid}", (Guid id) =>
        {
            // TODO: Implement get order by ID
            return Results.NotFound();
        })
        .RequireAuthorization()
        .WithName("GetOrderById")
        .WithSummary("Get order by ID");

        // Create order
        group.MapPost("/", () =>
        {
            // TODO: Implement create order
            return Results.Created();
        })
        .RequireAuthorization()
        .WithName("CreateOrder")
        .WithSummary("Create a new order");

        // Update order status
        group.MapPatch("/{id:guid}/status", (Guid id) =>
        {
            // TODO: Implement update order status
            return Results.Ok();
        })
        .RequireAuthorization()
        .WithName("UpdateOrderStatus")
        .WithSummary("Update order status");

        // Cancel order
        group.MapPost("/{id:guid}/cancel", (Guid id) =>
        {
            // TODO: Implement cancel order
            return Results.Ok();
        })
        .RequireAuthorization()
        .WithName("CancelOrder")
        .WithSummary("Cancel an order");

        // Get order payments
        group.MapGet("/{id:guid}/payments", (Guid id) =>
        {
            // TODO: Implement get order payments
            return Results.Ok(new { data = Array.Empty<object>() });
        })
        .RequireAuthorization()
        .WithName("GetOrderPayments")
        .WithSummary("Get payments for an order");
    }
}
