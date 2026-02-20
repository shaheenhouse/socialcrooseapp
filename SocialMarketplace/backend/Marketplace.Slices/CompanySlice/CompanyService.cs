using Dapper;
using Microsoft.Extensions.Logging;
using Marketplace.Core.Infrastructure;

namespace Marketplace.Slices.CompanySlice;

public interface ICompanyRepository
{
    Task<CompanyDto?> GetByIdAsync(Guid id);
    Task<CompanyDto?> GetBySlugAsync(string slug);
    Task<(IEnumerable<CompanyListDto> Companies, int TotalCount)> GetAllAsync(int page, int pageSize, string? search = null, string? industry = null);
    Task<Guid> CreateAsync(CreateCompanyDto dto, Guid ownerId);
    Task<bool> UpdateAsync(Guid id, UpdateCompanyDto dto);
    Task<IEnumerable<CompanyEmployeeDto>> GetEmployeesAsync(Guid companyId);
    Task<bool> AddEmployeeAsync(Guid companyId, Guid userId, string title, string? department);
    Task<bool> RemoveEmployeeAsync(Guid companyId, Guid userId);
    Task<(IEnumerable<JobDto> Jobs, int TotalCount)> GetJobsAsync(Guid companyId, int page, int pageSize);
}

public interface ICompanyService
{
    Task<CompanyDto?> GetByIdAsync(Guid id);
    Task<CompanyDto?> GetBySlugAsync(string slug);
    Task<(IEnumerable<CompanyListDto> Companies, int TotalCount)> GetAllAsync(int page, int pageSize, string? search = null, string? industry = null);
    Task<Guid> CreateAsync(CreateCompanyDto dto, Guid ownerId);
    Task<bool> UpdateAsync(Guid id, Guid ownerId, UpdateCompanyDto dto);
    Task<IEnumerable<CompanyEmployeeDto>> GetEmployeesAsync(Guid companyId);
    Task<bool> AddEmployeeAsync(Guid companyId, Guid ownerId, Guid userId, string title, string? department);
    Task<bool> RemoveEmployeeAsync(Guid companyId, Guid ownerId, Guid userId);
}

public class CompanyRepository : ICompanyRepository
{
    private readonly IConnectionFactory _connectionFactory;

    public CompanyRepository(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<CompanyDto?> GetByIdAsync(Guid id)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<CompanyDto>("""
            SELECT c.id, c."OwnerId", c.name, c.slug, c."LegalName",
                   c.description, c."LogoUrl", c."BannerUrl",
                   c.email, c.phone, c.website, c.address, c.city, c.country,
                   c."CompanyType", c.industry, c."FoundedYear",
                   c."CompanySize", c.status, c."IsVerified",
                   c.rating, c."TotalReviews", c."CreatedAt",
                   u."FirstName" || ' ' || u."LastName" as OwnerName
            FROM "Companies" c
            JOIN users u ON c."OwnerId" = u.id
            WHERE c.id = @Id AND c."IsDeleted" = false
            """, new { Id = id });
    }

    public async Task<CompanyDto?> GetBySlugAsync(string slug)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<CompanyDto>("""
            SELECT c.id, c."OwnerId", c.name, c.slug, c.description,
                   c."LogoUrl", c.industry, c."CompanySize",
                   c."IsVerified", c.rating, c."CreatedAt"
            FROM "Companies" c
            WHERE c.slug = @Slug AND c."IsDeleted" = false
            """, new { Slug = slug });
    }

    public async Task<(IEnumerable<CompanyListDto> Companies, int TotalCount)> GetAllAsync(int page, int pageSize, string? search = null, string? industry = null)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();

        var whereClause = @"WHERE c.""IsDeleted"" = false";
        if (!string.IsNullOrEmpty(search))
            whereClause += " AND (c.name ILIKE @Search OR c.description ILIKE @Search OR c.industry ILIKE @Search)";
        if (!string.IsNullOrEmpty(industry))
            whereClause += " AND c.industry ILIKE @Industry";

        var totalCount = await connection.ExecuteScalarAsync<int>(
            $"""SELECT COUNT(*) FROM "Companies" c {whereClause}""",
            new { Search = $"%{search}%", Industry = $"%{industry}%" });

        var companies = await connection.QueryAsync<CompanyListDto>($"""
            SELECT c.id, c.name, c.slug, c."LogoUrl", c.industry,
                   c."CompanySize", c.city, c.country,
                   c."IsVerified", c.rating, c."CreatedAt",
                   (SELECT COUNT(*) FROM "CompanyEmployees" ce WHERE ce."CompanyId" = c.id AND ce."IsActive" = true) as EmployeeCount
            FROM "Companies" c
            {whereClause}
            ORDER BY c."IsVerified" DESC, c.rating DESC, c."CreatedAt" DESC
            LIMIT @PageSize OFFSET @Offset
            """, new { Search = $"%{search}%", Industry = $"%{industry}%", PageSize = pageSize, Offset = (page - 1) * pageSize });

        return (companies, totalCount);
    }

    public async Task<Guid> CreateAsync(CreateCompanyDto dto, Guid ownerId)
    {
        using var connection = await _connectionFactory.CreateWriteConnectionAsync();
        var id = Guid.NewGuid();
        var slug = dto.Name.ToLower().Replace(" ", "-").Replace("'", "");

        await connection.ExecuteAsync("""
            INSERT INTO "Companies" (id, "OwnerId", name, slug, "LegalName", description, "LogoUrl", "BannerUrl",
                                  email, phone, website, address, city, country,
                                  "CompanyType", industry, "FoundedYear", "CompanySize", status,
                                  "IsVerified", "Rating", "TotalReviews", "TotalProjects", "TotalEmployees", "CommissionRate",
                                  "CreatedAt", "IsDeleted")
            VALUES (@Id, @OwnerId, @Name, @Slug, @LegalName, @Description, @LogoUrl, @BannerUrl,
                   @Email, @Phone, @Website, @Address, @City, @Country,
                   @CompanyType, @Industry, @FoundedYear, @CompanySize, 1,
                   false, 0, 0, 0, 0, 0,
                   NOW(), false)
            """, new
        {
            Id = id, OwnerId = ownerId, dto.Name, Slug = slug, dto.LegalName, dto.Description,
            dto.LogoUrl, dto.BannerUrl, dto.Email, dto.Phone, dto.Website, dto.Address,
            dto.City, dto.Country, dto.CompanyType, dto.Industry, dto.FoundedYear, dto.CompanySize
        });

        return id;
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateCompanyDto dto)
    {
        using var connection = await _connectionFactory.CreateWriteConnectionAsync();
        var updates = new List<string>();
        var parameters = new DynamicParameters();
        parameters.Add("Id", id);

        if (dto.Name != null) { updates.Add("name = @Name"); parameters.Add("Name", dto.Name); }
        if (dto.Description != null) { updates.Add("description = @Description"); parameters.Add("Description", dto.Description); }
        if (dto.LogoUrl != null) { updates.Add(@"""LogoUrl"" = @LogoUrl"); parameters.Add("LogoUrl", dto.LogoUrl); }
        if (dto.Industry != null) { updates.Add("industry = @Industry"); parameters.Add("Industry", dto.Industry); }
        if (dto.Website != null) { updates.Add("website = @Website"); parameters.Add("Website", dto.Website); }

        if (updates.Count == 0) return true;
        updates.Add(@"""UpdatedAt"" = NOW()");

        return await connection.ExecuteAsync(
            $"""UPDATE "Companies" SET {string.Join(", ", updates)} WHERE id = @Id AND "IsDeleted" = false""", parameters) > 0;
    }

    public async Task<IEnumerable<CompanyEmployeeDto>> GetEmployeesAsync(Guid companyId)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();
        return await connection.QueryAsync<CompanyEmployeeDto>("""
            SELECT ce.id, ce."UserId", ce.title, ce.department,
                   ce."IsActive", ce."JoinedAt",
                   u."FirstName", u."LastName",
                   u."AvatarUrl", u.email
            FROM "CompanyEmployees" ce
            JOIN users u ON ce."UserId" = u.id
            WHERE ce."CompanyId" = @CompanyId AND ce."IsActive" = true
            ORDER BY ce."JoinedAt"
            """, new { CompanyId = companyId });
    }

    public async Task<bool> AddEmployeeAsync(Guid companyId, Guid userId, string title, string? department)
    {
        using var connection = await _connectionFactory.CreateWriteConnectionAsync();
        var id = Guid.NewGuid();
        return await connection.ExecuteAsync("""
            INSERT INTO "CompanyEmployees" (id, "CompanyId", "UserId", "RoleId", title, department,
                                           "IsActive", "JoinedAt",
                                           "CanManageStores", "CanManageEmployees", "CanManageProjects", "CanManageFinances",
                                           "CreatedAt", "IsDeleted")
            VALUES (@Id, @CompanyId, @UserId, (SELECT "Id" FROM roles WHERE "IsDefault" = true LIMIT 1), @Title, @Department,
                   true, NOW(),
                   false, false, false, false,
                   NOW(), false)
            ON CONFLICT ("CompanyId", "UserId") DO UPDATE SET title = @Title, department = @Department, "IsActive" = true, "UpdatedAt" = NOW()
            """, new { Id = id, CompanyId = companyId, UserId = userId, Title = title, Department = department }) > 0;
    }

    public async Task<bool> RemoveEmployeeAsync(Guid companyId, Guid userId)
    {
        using var connection = await _connectionFactory.CreateWriteConnectionAsync();
        return await connection.ExecuteAsync(
            """UPDATE "CompanyEmployees" SET "IsActive" = false, "LeftAt" = NOW(), "UpdatedAt" = NOW() WHERE "CompanyId" = @CompanyId AND "UserId" = @UserId""",
            new { CompanyId = companyId, UserId = userId }) > 0;
    }

    public async Task<(IEnumerable<JobDto> Jobs, int TotalCount)> GetJobsAsync(Guid companyId, int page, int pageSize)
    {
        // Jobs are stored as projects with a specific type
        using var connection = await _connectionFactory.CreateReadConnectionAsync();

        var totalCount = await connection.ExecuteScalarAsync<int>(
            """SELECT COUNT(*) FROM projects WHERE client_id IN (SELECT "OwnerId" FROM "Companies" WHERE id = @CompanyId) AND project_type = 'Job' AND is_deleted = false""",
            new { CompanyId = companyId });

        var jobs = await connection.QueryAsync<JobDto>("""
            SELECT p.id, p.title, p.description, p.requirements,
                   p.budget_min as SalaryMin, p.budget_max as SalaryMax, p.currency,
                   p.experience_level as ExperienceLevel, p.visibility as EmploymentType,
                   p.tags as Skills, p.deadline as ApplicationDeadline,
                   p.status, p.created_at as PostedAt,
                   c.name as CompanyName, c."LogoUrl" as CompanyLogoUrl, c.city, c.country
            FROM projects p
            JOIN "Companies" c ON p.client_id = c."OwnerId" AND c.id = @CompanyId
            WHERE p.project_type = 'Job' AND p.is_deleted = false
            ORDER BY p.created_at DESC
            LIMIT @PageSize OFFSET @Offset
            """, new { CompanyId = companyId, PageSize = pageSize, Offset = (page - 1) * pageSize });

        return (jobs, totalCount);
    }
}

public class CompanyService : ICompanyService
{
    private readonly ICompanyRepository _repository;
    private readonly ILogger<CompanyService> _logger;

    public CompanyService(ICompanyRepository repository, ILogger<CompanyService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<CompanyDto?> GetByIdAsync(Guid id) => await _repository.GetByIdAsync(id);
    public async Task<CompanyDto?> GetBySlugAsync(string slug) => await _repository.GetBySlugAsync(slug);

    public async Task<(IEnumerable<CompanyListDto> Companies, int TotalCount)> GetAllAsync(int page, int pageSize, string? search, string? industry)
        => await _repository.GetAllAsync(page, pageSize, search, industry);

    public async Task<Guid> CreateAsync(CreateCompanyDto dto, Guid ownerId)
    {
        var id = await _repository.CreateAsync(dto, ownerId);
        _logger.LogInformation("Company created: {CompanyId} by {OwnerId}", id, ownerId);
        return id;
    }

    public async Task<bool> UpdateAsync(Guid id, Guid ownerId, UpdateCompanyDto dto)
    {
        var company = await _repository.GetByIdAsync(id);
        if (company == null || company.OwnerId != ownerId) return false;
        return await _repository.UpdateAsync(id, dto);
    }

    public async Task<IEnumerable<CompanyEmployeeDto>> GetEmployeesAsync(Guid companyId) => await _repository.GetEmployeesAsync(companyId);

    public async Task<bool> AddEmployeeAsync(Guid companyId, Guid ownerId, Guid userId, string title, string? department)
    {
        var company = await _repository.GetByIdAsync(companyId);
        if (company == null || company.OwnerId != ownerId) return false;
        return await _repository.AddEmployeeAsync(companyId, userId, title, department);
    }

    public async Task<bool> RemoveEmployeeAsync(Guid companyId, Guid ownerId, Guid userId)
    {
        var company = await _repository.GetByIdAsync(companyId);
        if (company == null || company.OwnerId != ownerId) return false;
        return await _repository.RemoveEmployeeAsync(companyId, userId);
    }
}

// DTOs
public record CompanyDto
{
    public Guid Id { get; init; }
    public Guid OwnerId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? LegalName { get; init; }
    public string? Description { get; init; }
    public string? LogoUrl { get; init; }
    public string? BannerUrl { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Website { get; init; }
    public string? Address { get; init; }
    public string? City { get; init; }
    public string? Country { get; init; }
    public string? CompanyType { get; init; }
    public string? Industry { get; init; }
    public int? FoundedYear { get; init; }
    public string? CompanySize { get; init; }
    public int Status { get; init; }
    public bool IsVerified { get; init; }
    public decimal Rating { get; init; }
    public int TotalReviews { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? OwnerName { get; init; }
}

public record CompanyListDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? LogoUrl { get; init; }
    public string? Industry { get; init; }
    public string? CompanySize { get; init; }
    public string? City { get; init; }
    public string? Country { get; init; }
    public bool IsVerified { get; init; }
    public decimal Rating { get; init; }
    public DateTime CreatedAt { get; init; }
    public int EmployeeCount { get; init; }
}

public record CreateCompanyDto(
    string Name,
    string? LegalName = null,
    string? Description = null,
    string? LogoUrl = null,
    string? BannerUrl = null,
    string? Email = null,
    string? Phone = null,
    string? Website = null,
    string? Address = null,
    string? City = null,
    string? Country = null,
    string? CompanyType = null,
    string? Industry = null,
    int? FoundedYear = null,
    string? CompanySize = null);

public record UpdateCompanyDto(
    string? Name = null,
    string? Description = null,
    string? LogoUrl = null,
    string? Industry = null,
    string? Website = null);

public record CompanyEmployeeDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string? Title { get; init; }
    public string? Department { get; init; }
    public bool IsActive { get; init; }
    public DateTime JoinedAt { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? AvatarUrl { get; init; }
    public string? Email { get; init; }
}

public record JobDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Requirements { get; init; }
    public decimal? SalaryMin { get; init; }
    public decimal? SalaryMax { get; init; }
    public string Currency { get; init; } = "USD";
    public string? ExperienceLevel { get; init; }
    public string? EmploymentType { get; init; }
    public string? Skills { get; init; }
    public DateTime? ApplicationDeadline { get; init; }
    public int Status { get; init; }
    public DateTime PostedAt { get; init; }
    public string? CompanyName { get; init; }
    public string? CompanyLogoUrl { get; init; }
    public string? City { get; init; }
    public string? Country { get; init; }
}
