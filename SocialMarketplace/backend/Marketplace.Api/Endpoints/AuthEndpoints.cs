using Microsoft.AspNetCore.Mvc;
using Marketplace.Slices.AuthSlice;

namespace Marketplace.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Authentication");

        group.MapPost("/login", async ([FromBody] LoginDto dto, IAuthService authService) =>
        {
            var result = await authService.LoginAsync(dto);
            
            if (!result.Success)
            {
                return Results.BadRequest(new { error = result.Error });
            }

            return Results.Ok(new
            {
                accessToken = result.AccessToken,
                refreshToken = result.RefreshToken,
                expiresAt = result.ExpiresAt,
                user = result.User
            });
        })
        .WithName("Login")
        .WithSummary("Authenticate user and get tokens")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest);

        group.MapPost("/register", async ([FromBody] RegisterDto dto, IAuthService authService) =>
        {
            var result = await authService.RegisterAsync(dto);
            
            if (!result.Success)
            {
                return Results.BadRequest(new { error = result.Error });
            }

            return Results.Created("/api/users/me", new
            {
                accessToken = result.AccessToken,
                refreshToken = result.RefreshToken,
                expiresAt = result.ExpiresAt,
                user = result.User
            });
        })
        .WithName("Register")
        .WithSummary("Register a new user")
        .Produces<object>(StatusCodes.Status201Created)
        .Produces<object>(StatusCodes.Status400BadRequest);

        group.MapPost("/refresh", async ([FromBody] RefreshTokenRequest request, IAuthService authService) =>
        {
            var result = await authService.RefreshTokenAsync(request.RefreshToken);
            
            if (!result.Success)
            {
                return Results.Unauthorized();
            }

            return Results.Ok(new
            {
                accessToken = result.AccessToken,
                refreshToken = result.RefreshToken,
                expiresAt = result.ExpiresAt,
                user = result.User
            });
        })
        .WithName("RefreshToken")
        .WithSummary("Refresh access token using refresh token")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/logout", async (HttpContext context, IAuthService authService) =>
        {
            var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Results.Unauthorized();
            }

            // Revoke all tokens for user
            await authService.RevokeAllTokensAsync(Guid.Parse(userId));
            return Results.Ok(new { message = "Logged out successfully" });
        })
        .RequireAuthorization()
        .WithName("Logout")
        .WithSummary("Logout and revoke all tokens")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}

public record RefreshTokenRequest(string RefreshToken);
