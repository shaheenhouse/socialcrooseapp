namespace Marketplace.Api.Endpoints;

public static class ProjectEndpoints
{
    public static void MapProjectEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/projects").WithTags("Projects");

        // Get all projects
        group.MapGet("/", () =>
        {
            // TODO: Implement project listing
            return Results.Ok(new { data = Array.Empty<object>(), pagination = new { page = 1, pageSize = 20, totalCount = 0 } });
        })
        .WithName("GetAllProjects")
        .WithSummary("Get all projects with pagination and filters");

        // Get project by ID
        group.MapGet("/{id:guid}", (Guid id) =>
        {
            // TODO: Implement get project by ID
            return Results.NotFound();
        })
        .WithName("GetProjectById")
        .WithSummary("Get project by ID");

        // Create project
        group.MapPost("/", () =>
        {
            // TODO: Implement create project
            return Results.Created();
        })
        .RequireAuthorization()
        .WithName("CreateProject")
        .WithSummary("Create a new project");

        // Update project
        group.MapPatch("/{id:guid}", (Guid id) =>
        {
            // TODO: Implement update project
            return Results.Ok();
        })
        .RequireAuthorization()
        .WithName("UpdateProject")
        .WithSummary("Update project");

        // Get project bids
        group.MapGet("/{id:guid}/bids", (Guid id) =>
        {
            // TODO: Implement get project bids
            return Results.Ok(new { data = Array.Empty<object>() });
        })
        .WithName("GetProjectBids")
        .WithSummary("Get bids for a project");

        // Submit bid
        group.MapPost("/{id:guid}/bids", (Guid id) =>
        {
            // TODO: Implement submit bid
            return Results.Created();
        })
        .RequireAuthorization()
        .WithName("SubmitBid")
        .WithSummary("Submit a bid for a project");

        // Get project milestones
        group.MapGet("/{id:guid}/milestones", (Guid id) =>
        {
            // TODO: Implement get project milestones
            return Results.Ok(new { data = Array.Empty<object>() });
        })
        .WithName("GetProjectMilestones")
        .WithSummary("Get milestones for a project");

        // Create milestone
        group.MapPost("/{id:guid}/milestones", (Guid id) =>
        {
            // TODO: Implement create milestone
            return Results.Created();
        })
        .RequireAuthorization()
        .WithName("CreateMilestone")
        .WithSummary("Create a milestone for a project");
    }
}
