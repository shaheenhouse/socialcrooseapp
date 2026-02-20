using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Marketplace.Slices.DesignSlice;

namespace Marketplace.Api.Endpoints;

public static class DesignEndpoints
{
    public static void MapDesignEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/designs").WithTags("Designs");

        group.MapGet("/me", async (HttpContext context,
            IDesignService designService,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var (designs, totalCount) = await designService.GetMyDesignsAsync(userId.Value, page, pageSize);
            return Results.Ok(new
            {
                data = designs,
                pagination = new { page, pageSize, totalCount, totalPages = (int)Math.Ceiling(totalCount / (double)pageSize) }
            });
        })
        .RequireAuthorization()
        .WithName("GetMyDesigns");

        group.MapGet("/templates", async (
            IDesignService designService,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? category = null) =>
        {
            var (templates, totalCount) = await designService.GetTemplatesAsync(page, pageSize, category);
            return Results.Ok(new
            {
                data = templates,
                pagination = new { page, pageSize, totalCount, totalPages = (int)Math.Ceiling(totalCount / (double)pageSize) }
            });
        })
        .WithName("GetDesignTemplates");

        group.MapGet("/{id:guid}", async (HttpContext context, Guid id, IDesignService designService) =>
        {
            var design = await designService.GetByIdAsync(id);
            if (design == null) return Results.NotFound();

            var userId = GetUserId(context);
            if (!design.IsPublic && design.UserId != userId)
                return Results.NotFound();

            return Results.Ok(design);
        })
        .WithName("GetDesignById");

        group.MapPost("/", async (HttpContext context, [FromBody] CreateDesignDto dto, IDesignService designService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var id = await designService.CreateAsync(dto, userId.Value);
            return Results.Created($"/api/designs/{id}", new { id });
        })
        .RequireAuthorization()
        .WithName("CreateDesign");

        group.MapPost("/{id:guid}/duplicate", async (HttpContext context, Guid id, IDesignService designService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            try
            {
                var newId = await designService.DuplicateAsync(id, userId.Value);
                return Results.Created($"/api/designs/{newId}", new { id = newId });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .RequireAuthorization()
        .WithName("DuplicateDesign");

        group.MapPatch("/{id:guid}", async (HttpContext context, Guid id, [FromBody] UpdateDesignDto dto, IDesignService designService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var result = await designService.UpdateAsync(id, userId.Value, dto);
            return result ? Results.Ok(new { message = "Design updated" }) : Results.NotFound();
        })
        .RequireAuthorization()
        .WithName("UpdateDesign");

        group.MapDelete("/{id:guid}", async (HttpContext context, Guid id, IDesignService designService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var result = await designService.DeleteAsync(id, userId.Value);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .RequireAuthorization()
        .WithName("DeleteDesign");
    }

    private static Guid? GetUserId(HttpContext context)
    {
        var claim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return claim != null && Guid.TryParse(claim, out var id) ? id : null;
    }
}
