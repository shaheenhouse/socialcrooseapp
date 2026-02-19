using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Marketplace.Slices.WalletSlice;

namespace Marketplace.Api.Endpoints;

public static class WalletEndpoints
{
    public static void MapWalletEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/wallet").WithTags("Wallet").RequireAuthorization();

        group.MapGet("/", async (HttpContext context, IWalletService walletService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var wallet = await walletService.GetMyWalletAsync(userId.Value);
            return wallet != null ? Results.Ok(wallet) : Results.NotFound();
        })
        .WithName("GetMyWallet");

        group.MapGet("/transactions", async (HttpContext context,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? type = null,
            IWalletService walletService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var (transactions, totalCount) = await walletService.GetTransactionsAsync(userId.Value, page, pageSize, type);
            return Results.Ok(new
            {
                data = transactions,
                pagination = new { page, pageSize, totalCount, totalPages = (int)Math.Ceiling(totalCount / (double)pageSize) }
            });
        })
        .WithName("GetWalletTransactions");

        group.MapGet("/escrows", async (HttpContext context,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            IWalletService walletService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var escrows = await walletService.GetEscrowsAsync(userId.Value, page, pageSize);
            return Results.Ok(new { data = escrows });
        })
        .WithName("GetMyEscrows");
    }

    private static Guid? GetUserId(HttpContext context)
    {
        var claim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return claim != null && Guid.TryParse(claim, out var id) ? id : null;
    }
}
