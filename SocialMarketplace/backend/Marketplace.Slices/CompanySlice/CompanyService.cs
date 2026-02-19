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
            SELECT c.id, c.owner_id as OwnerId, c.name, c.slug, c.legal_name as LegalName,
                   c.description, c.logo_url as LogoUrl, c.banner_url as BannerUrl,
                   c.email, c.phone, c.website, c.address, c.city, c.country,
                   c.company_type as CompanyType, c.industry, c.founded_year as FoundedYear,
                   c.company_size as CompanySize, c.status, c.is_verified as IsVerified,
                   c.rating, c.total_reviews as TotalReviews, c.created_at as CreatedAt,
                   u.first_name || ' ' || u.last_name as OwnerName
            FROM companies c
            JOIN users u ON c.owner_id = u.id
            WHERE c.id = @Id AND c.is_deleted = false
            """, new { Id = id });
    }

    public async Task<CompanyDto?> GetBySlugAsync(string slug)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<CompanyDto>("""
            SELECT c.id, c.owner_id as OwnerId, c.name, c.slug, c.description,
                   c.logo_url as LogoUrl, c.industry, c.company_size as CompanySize,
                   c.is_verified as IsVerified, c.rating, c.created_at as CreatedAt
            FROM companies c
            WHERE c.slug = @Slug AND c.is_deleted = false
            """, new { Slug = slug });
    }

    public async Task<(IEnumerable<CompanyListDto> Companies, int TotalCount)> GetAllAsync(int page, int pageSize, string? search = null, string? industry = null)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();

        var whereClause = "WHERE c.is_deleted = false";
        if (!string.IsNullOrEmpty(search))
            whereClause += " AND (c.name ILIKE @Search OR c.description ILIKE @Search OR c.industry ILIKE @Search)";
        if (!string.IsNullOrEmpty(industry))
            whereClause += " AND c.industry ILIKE @Industry";

        var totalCount = await connection.ExecuteScalarAsync<int>(
            $"SELECT COUNT(*) FROM companies c {whereClause}",
            new { Search = $"%{search}%", Industry = $"%{industry}%" });

        var companies = await connection.QueryAsync<CompanyListDto>($"""
            SELECT c.id, c.name, c.slug, c.logo_url as LogoUrl, c.industry,
                   c.company_size as CompanySize, c.city, c.country,
                   c.is_verified as IsVerified, c.rating, c.created_at as CreatedAt,
                   (SELECT COUNT(*) FROM company_employees ce WHERE ce.company_id = c.id AND ce.is_active = true) as EmployeeCount
            FROM companies c
            {whereClause}
            ORDER BY c.is_verified DESC, c.rating DESC, c.created_at DESC
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
            INSERT INTO companies (id, owner_id, name, slug, legal_name, description, logo_url, banner_url,
                                  email, phone, website, address, city, country,
                                  company_type, industry, founded_year, company_size, status,
                                  created_at, is_deleted)
            VALUES (@Id, @OwnerId, @Name, @Slug, @LegalName, @Description, @LogoUrl, @BannerUrl,
                   @Email, @Phone, @Website, @Address, @City, @Country,
                   @CompanyType, @Industry, @FoundedYear, @CompanySize, 1,
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
        if (dto.LogoUrl != null) { updates.Add("logo_url = @LogoUrl"); parameters.Add("LogoUrl", dto.LogoUrl); }
        if (dto.Industry != null) { updates.Add("industry = @Industry"); parameters.Add("Industry", dto.Industry); }
        if (dto.Website != null) { updates.Add("website = @Website"); parameters.Add("Website", dto.Website); }

        if (updates.Count == 0) return true;
        updates.Add("updated_at = NOW()");

        return await connection.ExecuteAsync(
            $"UPDATE companies SET {string.Join(", ", updates)} WHERE id = @Id AND is_deleted = false", parameters) > 0;
    }

    public async Task<IEnumerable<CompanyEmployeeDto>> GetEmployeesAsync(Guid companyId)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();
        return await connection.QueryAsync<CompanyEmployeeDto>("""
            SELECT ce.id, ce.user_id as UserId, ce.title, ce.department,
                   ce.is_active as IsActive, ce.joined_at as JoinedAt,
                   u.first_name as FirstName, u.last_name as LastName,
                   u.avatar_url as AvatarUrl, u.email
            FROM company_employees ce
            JOIN users u ON ce.user_id = u.id
            WHERE ce.company_id = @CompanyId AND ce.is_active = true
            ORDER BY ce.joined_at
            """, new { CompanyId = companyId });
    }

    public async Task<bool> AddEmployeeAsync(Guid companyId, Guid userId, string title, string? department)
    {
        using var connection = await _connectionFactory.CreateWriteConnectionAsync();
        var id = Guid.NewGuid();
        return await connection.ExecuteAsync("""
            INSERT INTO company_employees (id, company_id, user_id, title, department, is_active, joined_at, created_at, is_deleted)
            VALUES (@Id, @CompanyId, @UserId, @Title, @Department, true, NOW(), NOW(), false)
            ON CONFLICT (company_id, user_id) DO UPDATE SET title = @Title, department = @Department, is_active = true, updated_at = NOW()
            """, new { Id = id, CompanyId = companyId, UserId = userId, Title = title, Department = department }) > 0;
    }

    public async Task<bool> RemoveEmployeeAsync(Guid companyId, Guid userId)
    {
        using var connection = await _connectionFactory.CreateWriteConnectionAsync();
        return await connection.ExecuteAsync(
            "UPDATE company_employees SET is_active = false, left_at = NOW(), updated_at = NOW() WHERE company_id = @CompanyId AND user_id = @UserId",
            new { CompanyId = companyId, UserId = userId }) > 0;
    }

    public async Task<(IEnumerable<JobDto> Jobs, int TotalCount)> GetJobsAsync(Guid companyId, int page, int pageSize)
    {
        // Jobs are stored as projects with a specific type
        using var connection = await _connectionFactory.CreateReadConnectionAsync();

        var totalCount = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM projects WHERE client_id IN (SELECT owner_id FROM companies WHERE id = @CompanyId) AND project_type = 'Job' AND is_deleted = false",
            new { CompanyId = companyId });

        var jobs = await connection.QueryAsync<JobDto>("""
            SELECT p.id, p.title, p.description, p.requirements,
                   p.budget_min as SalaryMin, p.budget_max as SalaryMax, p.currency,
                   p.experience_level as ExperienceLevel, p.visibility as EmploymentType,
                   p.tags as Skills, p.deadline as ApplicationDeadline,
                   p.status, p.created_at as PostedAt,
                   c.name as CompanyName, c.logo_url as CompanyLogoUrl, c.city, c.country
            FROM projects p
            JOIN companies c ON p.client_id = c.owner_id AND c.id = @CompanyId
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
