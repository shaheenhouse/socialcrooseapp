using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Marketplace.Slices.ProjectSlice;

namespace Marketplace.Api.Endpoints;

public static class ProjectEndpoints
{
    public static void MapProjectEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/projects").WithTags("Projects");

        group.MapGet("/", async (
            IProjectService projectService,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] string? status = null,
            [FromQuery] Guid? category = null,
            [FromQuery] decimal? minBudget = null,
            [FromQuery] decimal? maxBudget = null) =>
        {
            var query = new ProjectQueryParams
            {
                Page = page, PageSize = pageSize, Search = search, Status = status,
                CategoryId = category, MinBudget = minBudget, MaxBudget = maxBudget
            };
            var (projects, totalCount) = await projectService.GetAllAsync(query);
            return Results.Ok(new
            {
                data = projects,
                pagination = new { page, pageSize, totalCount, totalPages = (int)Math.Ceiling(totalCount / (double)pageSize) }
            });
        })
        .WithName("GetAllProjects");

        group.MapGet("/my", async (HttpContext context,
            IProjectService projectService,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var (projects, totalCount) = await projectService.GetMyProjectsAsync(userId.Value, page, pageSize);
            return Results.Ok(new
            {
                data = projects,
                pagination = new { page, pageSize, totalCount, totalPages = (int)Math.Ceiling(totalCount / (double)pageSize) }
            });
        })
        .RequireAuthorization()
        .WithName("GetMyProjects");

        group.MapGet("/{id:guid}", async (Guid id, IProjectService projectService) =>
        {
            var project = await projectService.GetByIdAsync(id);
            return project != null ? Results.Ok(project) : Results.NotFound();
        })
        .WithName("GetProjectById");

        group.MapPost("/", async (HttpContext context, [FromBody] CreateProjectDto dto, IProjectService projectService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var id = await projectService.CreateAsync(dto, userId.Value);
            return Results.Created($"/api/projects/{id}", new { id });
        })
        .RequireAuthorization()
        .WithName("CreateProject");

        group.MapPatch("/{id:guid}", async (HttpContext context, Guid id, [FromBody] UpdateProjectDto dto, IProjectService projectService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var result = await projectService.UpdateAsync(id, userId.Value, dto);
            return result ? Results.Ok(new { message = "Project updated" }) : Results.NotFound();
        })
        .RequireAuthorization()
        .WithName("UpdateProject");

        group.MapGet("/{id:guid}/bids", async (Guid id,
            IProjectService projectService,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20) =>
        {
            var bids = await projectService.GetBidsAsync(id, page, pageSize);
            return Results.Ok(new { data = bids });
        })
        .WithName("GetProjectBids");

        group.MapPost("/{id:guid}/bids", async (HttpContext context, Guid id, [FromBody] CreateBidRequest request, IProjectService projectService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var dto = new CreateBidDto(id, request.Amount, request.Currency, request.DeliveryDays, request.Proposal);
            var bidId = await projectService.SubmitBidAsync(dto, userId.Value);
            return Results.Created($"/api/projects/{id}/bids/{bidId}", new { id = bidId });
        })
        .RequireAuthorization()
        .WithName("SubmitBid");

        group.MapGet("/{id:guid}/milestones", async (Guid id, IProjectService projectService) =>
        {
            var milestones = await projectService.GetMilestonesAsync(id);
            return Results.Ok(new { data = milestones });
        })
        .WithName("GetProjectMilestones");

        group.MapPost("/{id:guid}/milestones", async (HttpContext context, Guid id, [FromBody] CreateMilestoneRequest request, IProjectService projectService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var dto = new CreateMilestoneDto(id, request.Title, request.Description, request.Amount, request.Currency, request.DueDate);
            var milestoneId = await projectService.CreateMilestoneAsync(dto, userId.Value);
            return Results.Created($"/api/projects/{id}/milestones/{milestoneId}", new { id = milestoneId });
        })
        .RequireAuthorization()
        .WithName("CreateMilestone");
    }

    private static Guid? GetUserId(HttpContext context)
    {
        var claim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return claim != null && Guid.TryParse(claim, out var id) ? id : null;
    }
}

public record CreateBidRequest(decimal Amount, string? Currency = "USD", int DeliveryDays = 7, string? Proposal = null);
public record CreateMilestoneRequest(string Title, string? Description = null, decimal Amount = 0, string? Currency = "USD", DateTime? DueDate = null);
