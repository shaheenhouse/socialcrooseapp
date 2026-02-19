using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Marketplace.Slices.CompanySlice;

namespace Marketplace.Api.Endpoints;

public static class CompanyEndpoints
{
    public static void MapCompanyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/companies").WithTags("Companies");

        group.MapGet("/", async (
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] string? industry = null,
            ICompanyService companyService) =>
        {
            var (companies, totalCount) = await companyService.GetAllAsync(page, pageSize, search, industry);
            return Results.Ok(new
            {
                data = companies,
                pagination = new { page, pageSize, totalCount, totalPages = (int)Math.Ceiling(totalCount / (double)pageSize) }
            });
        })
        .WithName("GetAllCompanies");

        group.MapGet("/{id:guid}", async (Guid id, ICompanyService companyService) =>
        {
            var company = await companyService.GetByIdAsync(id);
            return company != null ? Results.Ok(company) : Results.NotFound();
        })
        .WithName("GetCompanyById");

        group.MapGet("/slug/{slug}", async (string slug, ICompanyService companyService) =>
        {
            var company = await companyService.GetBySlugAsync(slug);
            return company != null ? Results.Ok(company) : Results.NotFound();
        })
        .WithName("GetCompanyBySlug");

        group.MapPost("/", async (HttpContext context, [FromBody] CreateCompanyDto dto, ICompanyService companyService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var id = await companyService.CreateAsync(dto, userId.Value);
            return Results.Created($"/api/companies/{id}", new { id });
        })
        .RequireAuthorization()
        .WithName("CreateCompany");

        group.MapPatch("/{id:guid}", async (HttpContext context, Guid id, [FromBody] UpdateCompanyDto dto, ICompanyService companyService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var result = await companyService.UpdateAsync(id, userId.Value, dto);
            return result ? Results.Ok(new { message = "Company updated" }) : Results.NotFound();
        })
        .RequireAuthorization()
        .WithName("UpdateCompany");

        group.MapGet("/{id:guid}/employees", async (Guid id, ICompanyService companyService) =>
        {
            var employees = await companyService.GetEmployeesAsync(id);
            return Results.Ok(new { data = employees });
        })
        .WithName("GetCompanyEmployees");

        group.MapPost("/{id:guid}/employees", async (HttpContext context, Guid id,
            [FromBody] AddEmployeeRequest request, ICompanyService companyService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var result = await companyService.AddEmployeeAsync(id, userId.Value, request.UserId, request.Title, request.Department);
            return result ? Results.Created() : Results.BadRequest();
        })
        .RequireAuthorization()
        .WithName("AddCompanyEmployee");

        group.MapDelete("/{id:guid}/employees/{employeeUserId:guid}", async (HttpContext context, Guid id, Guid employeeUserId, ICompanyService companyService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var result = await companyService.RemoveEmployeeAsync(id, userId.Value, employeeUserId);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .RequireAuthorization()
        .WithName("RemoveCompanyEmployee");
    }

    private static Guid? GetUserId(HttpContext context)
    {
        var claim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return claim != null && Guid.TryParse(claim, out var id) ? id : null;
    }
}

public record AddEmployeeRequest(Guid UserId, string Title, string? Department = null);
