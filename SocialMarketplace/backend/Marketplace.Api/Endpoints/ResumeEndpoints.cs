using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Marketplace.Slices.ResumeSlice;

namespace Marketplace.Api.Endpoints;

public static class ResumeEndpoints
{
    public static void MapResumeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/resumes").WithTags("Resumes");

        group.MapGet("/me", async (HttpContext context, IResumeService resumeService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var resumes = await resumeService.GetMyResumesAsync(userId.Value);
            return Results.Ok(new { data = resumes });
        })
        .RequireAuthorization()
        .WithName("GetMyResumes");

        group.MapGet("/{id:guid}", async (Guid id, IResumeService resumeService) =>
        {
            var resume = await resumeService.GetByIdAsync(id);
            return resume != null ? Results.Ok(resume) : Results.NotFound();
        })
        .WithName("GetResumeById");

        group.MapPost("/", async (HttpContext context, [FromBody] CreateResumeDto dto, IResumeService resumeService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var id = await resumeService.CreateAsync(dto, userId.Value);
            return Results.Created($"/api/resumes/{id}", new { id });
        })
        .RequireAuthorization()
        .WithName("CreateResume");

        group.MapPatch("/{id:guid}", async (HttpContext context, Guid id, [FromBody] UpdateResumeDto dto, IResumeService resumeService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var result = await resumeService.UpdateAsync(id, userId.Value, dto);
            return result ? Results.Ok(new { message = "Resume updated" }) : Results.NotFound();
        })
        .RequireAuthorization()
        .WithName("UpdateResume");

        group.MapDelete("/{id:guid}", async (HttpContext context, Guid id, IResumeService resumeService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var result = await resumeService.DeleteAsync(id, userId.Value);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .RequireAuthorization()
        .WithName("DeleteResume");
    }

    private static Guid? GetUserId(HttpContext context)
    {
        var claim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return claim != null && Guid.TryParse(claim, out var id) ? id : null;
    }
}
