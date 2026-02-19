using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Marketplace.Slices.UserSlice.DTO;
using Marketplace.Slices.UserSlice.Services;

namespace Marketplace.Api.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users").WithTags("Users");

        // Get current user
        group.MapGet("/me", async (HttpContext context, IUserService userService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();

            var user = await userService.GetByIdAsync(userId.Value);
            return user != null ? Results.Ok(user) : Results.NotFound();
        })
        .RequireAuthorization()
        .WithName("GetCurrentUser")
        .WithSummary("Get current authenticated user")
        .Produces<UserDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);

        // Get user by ID
        group.MapGet("/{id:guid}", async (Guid id, IUserService userService) =>
        {
            var user = await userService.GetByIdAsync(id);
            return user != null ? Results.Ok(user) : Results.NotFound();
        })
        .WithName("GetUserById")
        .WithSummary("Get user by ID")
        .Produces<UserDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // Get all users (paginated)
        group.MapGet("/", async (
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] string? status = null,
            IUserService userService) =>
        {
            var (users, totalCount) = await userService.GetAllAsync(page, pageSize, search, status);
            return Results.Ok(new
            {
                data = users,
                pagination = new
                {
                    page,
                    pageSize,
                    totalCount,
                    totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                }
            });
        })
        .WithName("GetAllUsers")
        .WithSummary("Get all users with pagination")
        .Produces<object>(StatusCodes.Status200OK);

        // Update current user
        group.MapPatch("/me", async (HttpContext context, [FromBody] UpdateUserDto dto, IUserService userService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();

            var result = await userService.UpdateAsync(userId.Value, dto);
            return result ? Results.Ok(new { message = "User updated successfully" }) : Results.NotFound();
        })
        .RequireAuthorization()
        .WithName("UpdateCurrentUser")
        .WithSummary("Update current user")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);

        // Get user profile
        group.MapGet("/{id:guid}/profile", async (Guid id, IUserService userService) =>
        {
            var profile = await userService.GetProfileAsync(id);
            return profile != null ? Results.Ok(profile) : Results.NotFound();
        })
        .WithName("GetUserProfile")
        .WithSummary("Get user profile")
        .Produces<UserProfileDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // Update user profile
        group.MapPatch("/me/profile", async (HttpContext context, [FromBody] UpdateProfileDto dto, IUserService userService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();

            var result = await userService.UpdateProfileAsync(userId.Value, dto);
            return result ? Results.Ok(new { message = "Profile updated successfully" }) : Results.BadRequest();
        })
        .RequireAuthorization()
        .WithName("UpdateUserProfile")
        .WithSummary("Update current user profile")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);

        // Get user skills
        group.MapGet("/{id:guid}/skills", async (Guid id, IUserService userService) =>
        {
            var skills = await userService.GetUserSkillsAsync(id);
            return Results.Ok(skills);
        })
        .WithName("GetUserSkills")
        .WithSummary("Get user skills")
        .Produces<IEnumerable<UserSkillDto>>(StatusCodes.Status200OK);

        // Add skill to current user
        group.MapPost("/me/skills", async (HttpContext context, [FromBody] AddSkillRequest request, IUserService userService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();

            var result = await userService.AddUserSkillAsync(userId.Value, request.SkillId, request.Level, request.YearsOfExperience);
            return result ? Results.Created() : Results.BadRequest();
        })
        .RequireAuthorization()
        .WithName("AddUserSkill")
        .WithSummary("Add skill to current user")
        .Produces(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status401Unauthorized);

        // Remove skill from current user
        group.MapDelete("/me/skills/{skillId:guid}", async (HttpContext context, Guid skillId, IUserService userService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();

            var result = await userService.RemoveUserSkillAsync(userId.Value, skillId);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .RequireAuthorization()
        .WithName("RemoveUserSkill")
        .WithSummary("Remove skill from current user")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status401Unauthorized);
    }

    private static Guid? GetUserId(HttpContext context)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return userIdClaim != null && Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}

public record AddSkillRequest(Guid SkillId, string Level, int YearsOfExperience);
