using Dapper;
using Marketplace.Core.Infrastructure;
using Marketplace.Database.Enums;

namespace Marketplace.Slices.StoreSlice;

public interface IStoreRepository
{
    Task<StoreDto?> GetByIdAsync(Guid id);
    Task<StoreDto?> GetBySlugAsync(string slug);
    Task<StoreDto?> GetByOwnerIdAsync(Guid ownerId);
    Task<(IEnumerable<StoreListDto> Stores, int TotalCount)> GetAllAsync(int page, int pageSize, string? search = null, string? status = null);
    Task<Guid> CreateAsync(CreateStoreDto dto, Guid ownerId);
    Task<bool> UpdateAsync(Guid id, UpdateStoreDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<StoreEmployeeDto>> GetEmployeesAsync(Guid storeId);
    Task<StoreAnalyticsDto> GetAnalyticsAsync(Guid storeId);
}

public class StoreRepository : IStoreRepository
{
    private readonly IConnectionFactory _connectionFactory;

    public StoreRepository(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<StoreDto?> GetByIdAsync(Guid id)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();

        const string sql = """
            SELECT s."Id", s."OwnerId", s."Name", s."Slug", s."Description", s."ShortDescription",
                   s."LogoUrl", s."BannerUrl", s."Status", s."Email", s."Phone", s."Website",
                   s."Address", s."City", s."State", s."Country", s."PostalCode",
                   s."CommissionRate", s."Rating", s."TotalReviews",
                   s."TotalProducts", s."TotalOrders", s."TotalSales",
                   s."IsVerified", s."IsFeatured",
                   s."SocialLinks", s."BusinessHours",
                   s."ShippingPolicy", s."ReturnPolicy",
                   s."CreatedAt",
                   u."FirstName" || ' ' || u."LastName" as OwnerName, u."AvatarUrl" as OwnerAvatarUrl
            FROM stores s
            JOIN users u ON s."OwnerId" = u."Id"
            WHERE s."Id" = @Id AND s."IsDeleted" = false
            """;

        return await connection.QuerySingleOrDefaultAsync<StoreDto>(sql, new { Id = id });
    }

    public async Task<StoreDto?> GetBySlugAsync(string slug)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();

        const string sql = """
            SELECT s."Id", s."OwnerId", s."Name", s."Slug", s."Description", s."ShortDescription",
                   s."LogoUrl", s."BannerUrl", s."Status", s."Email", s."Phone", s."Website",
                   s."Address", s."City", s."State", s."Country", s."PostalCode",
                   s."CommissionRate", s."Rating", s."TotalReviews",
                   s."TotalProducts", s."TotalOrders", s."TotalSales",
                   s."IsVerified", s."IsFeatured",
                   s."SocialLinks", s."BusinessHours",
                   s."ShippingPolicy", s."ReturnPolicy",
                   s."CreatedAt",
                   u."FirstName" || ' ' || u."LastName" as OwnerName, u."AvatarUrl" as OwnerAvatarUrl
            FROM stores s
            JOIN users u ON s."OwnerId" = u."Id"
            WHERE s."Slug" = @Slug AND s."IsDeleted" = false
            """;

        return await connection.QuerySingleOrDefaultAsync<StoreDto>(sql, new { Slug = slug });
    }

    public async Task<StoreDto?> GetByOwnerIdAsync(Guid ownerId)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();

        const string sql = """
            SELECT s."Id", s."OwnerId", s."Name", s."Slug", s."Description",
                   s."LogoUrl", s."BannerUrl", s."Status",
                   s."Rating", s."TotalReviews", s."TotalProducts",
                   s."TotalOrders", s."TotalSales",
                   s."IsVerified", s."CreatedAt"
            FROM stores s
            WHERE s."OwnerId" = @OwnerId AND s."IsDeleted" = false
            LIMIT 1
            """;

        return await connection.QuerySingleOrDefaultAsync<StoreDto>(sql, new { OwnerId = ownerId });
    }

    public async Task<(IEnumerable<StoreListDto> Stores, int TotalCount)> GetAllAsync(int page, int pageSize, string? search = null, string? status = null)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();

        var whereClause = "WHERE s.\"IsDeleted\" = false";
        if (!string.IsNullOrEmpty(search))
            whereClause += " AND (s.\"Name\" ILIKE @Search OR s.\"Description\" ILIKE @Search)";
        if (!string.IsNullOrEmpty(status))
            whereClause += " AND s.\"Status\" = @Status::integer";

        var countSql = $"SELECT COUNT(*) FROM stores s {whereClause}";
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, new { Search = $"%{search}%", Status = status });

        var sql = $"""
            SELECT s."Id", s."Name", s."Slug", s."LogoUrl", s."ShortDescription",
                   s."Status", s."Rating", s."TotalReviews", s."TotalProducts",
                   s."TotalOrders", s."IsVerified", s."IsFeatured",
                   s."City", s."Country", s."CreatedAt",
                   u."FirstName" || ' ' || u."LastName" as OwnerName
            FROM stores s
            JOIN users u ON s."OwnerId" = u."Id"
            {whereClause}
            ORDER BY s."IsFeatured" DESC, s."Rating" DESC, s."CreatedAt" DESC
            LIMIT @PageSize OFFSET @Offset
            """;

        var stores = await connection.QueryAsync<StoreListDto>(sql, new
        {
            Search = $"%{search}%",
            Status = status,
            PageSize = pageSize,
            Offset = (page - 1) * pageSize
        });

        return (stores, totalCount);
    }

    public async Task<Guid> CreateAsync(CreateStoreDto dto, Guid ownerId)
    {
        using var connection = await _connectionFactory.CreateWriteConnectionAsync();

        var id = Guid.NewGuid();
        var slug = GenerateSlug(dto.Name);

        const string sql = """
            INSERT INTO stores ("Id", "OwnerId", "Name", "Slug", "Description", "ShortDescription", "LogoUrl", "BannerUrl",
                               "Email", "Phone", "Website", "Address", "City", "State", "Country", "PostalCode",
                               "Status", "CommissionRate", "Rating", "TotalReviews", "TotalProducts",
                               "TotalOrders", "TotalSales", "IsVerified", "IsFeatured",
                               "ShippingPolicy", "ReturnPolicy", "CreatedAt", "IsDeleted")
            VALUES (@Id, @OwnerId, @Name, @Slug, @Description, @ShortDescription, @LogoUrl, @BannerUrl,
                   @Email, @Phone, @Website, @Address, @City, @State, @Country, @PostalCode,
                   @Status, 10.0, 0, 0, 0,
                   0, 0, false, false,
                   @ShippingPolicy, @ReturnPolicy, NOW(), false)
            RETURNING "Id"
            """;

        return await connection.ExecuteScalarAsync<Guid>(sql, new
        {
            Id = id,
            OwnerId = ownerId,
            dto.Name,
            Slug = slug,
            dto.Description,
            dto.ShortDescription,
            dto.LogoUrl,
            dto.BannerUrl,
            dto.Email,
            dto.Phone,
            dto.Website,
            dto.Address,
            dto.City,
            dto.State,
            dto.Country,
            dto.PostalCode,
            Status = (int)StoreStatus.Pending,
            dto.ShippingPolicy,
            dto.ReturnPolicy
        });
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateStoreDto dto)
    {
        using var connection = await _connectionFactory.CreateWriteConnectionAsync();

        var updates = new List<string>();
        var parameters = new DynamicParameters();
        parameters.Add("Id", id);

        if (dto.Name != null) { updates.Add("\"Name\" = @Name"); parameters.Add("Name", dto.Name); }
        if (dto.Description != null) { updates.Add("\"Description\" = @Description"); parameters.Add("Description", dto.Description); }
        if (dto.ShortDescription != null) { updates.Add("\"ShortDescription\" = @ShortDescription"); parameters.Add("ShortDescription", dto.ShortDescription); }
        if (dto.LogoUrl != null) { updates.Add("\"LogoUrl\" = @LogoUrl"); parameters.Add("LogoUrl", dto.LogoUrl); }
        if (dto.BannerUrl != null) { updates.Add("\"BannerUrl\" = @BannerUrl"); parameters.Add("BannerUrl", dto.BannerUrl); }
        if (dto.Email != null) { updates.Add("\"Email\" = @Email"); parameters.Add("Email", dto.Email); }
        if (dto.Phone != null) { updates.Add("\"Phone\" = @Phone"); parameters.Add("Phone", dto.Phone); }
        if (dto.Website != null) { updates.Add("\"Website\" = @Website"); parameters.Add("Website", dto.Website); }
        if (dto.Address != null) { updates.Add("\"Address\" = @Address"); parameters.Add("Address", dto.Address); }
        if (dto.City != null) { updates.Add("\"City\" = @City"); parameters.Add("City", dto.City); }
        if (dto.Country != null) { updates.Add("\"Country\" = @Country"); parameters.Add("Country", dto.Country); }
        if (dto.ShippingPolicy != null) { updates.Add("\"ShippingPolicy\" = @ShippingPolicy"); parameters.Add("ShippingPolicy", dto.ShippingPolicy); }
        if (dto.ReturnPolicy != null) { updates.Add("\"ReturnPolicy\" = @ReturnPolicy"); parameters.Add("ReturnPolicy", dto.ReturnPolicy); }

        if (updates.Count == 0) return true;
        updates.Add("\"UpdatedAt\" = NOW()");

        var sql = $"UPDATE stores SET {string.Join(", ", updates)} WHERE \"Id\" = @Id AND \"IsDeleted\" = false";
        return await connection.ExecuteAsync(sql, parameters) > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        using var connection = await _connectionFactory.CreateWriteConnectionAsync();
        return await connection.ExecuteAsync(
            "UPDATE stores SET \"IsDeleted\" = true, \"DeletedAt\" = NOW() WHERE \"Id\" = @Id", new { Id = id }) > 0;
    }

    public async Task<IEnumerable<StoreEmployeeDto>> GetEmployeesAsync(Guid storeId)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();

        const string sql = """
            SELECT se."Id", se."UserId", se."Title", se."Department",
                   se."IsActive", se."JoinedAt",
                   u."FirstName", u."LastName",
                   u."AvatarUrl", u."Email"
            FROM store_employees se
            JOIN users u ON se."UserId" = u."Id"
            WHERE se."StoreId" = @StoreId AND se."IsActive" = true
            ORDER BY se."JoinedAt"
            """;

        return await connection.QueryAsync<StoreEmployeeDto>(sql, new { StoreId = storeId });
    }

    public async Task<StoreAnalyticsDto> GetAnalyticsAsync(Guid storeId)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();

        const string sql = """
            SELECT 
                s."TotalProducts",
                s."TotalOrders",
                s."TotalSales",
                s."TotalReviews",
                s."Rating" as AverageRating,
                (SELECT COUNT(*) FROM orders o WHERE o."StoreId" = @StoreId AND o."Status" = 0 AND o."IsDeleted" = false) as PendingOrders,
                (SELECT COUNT(*) FROM orders o WHERE o."StoreId" = @StoreId AND o."CreatedAt" >= NOW() - INTERVAL '30 days' AND o."IsDeleted" = false) as OrdersThisMonth,
                (SELECT COALESCE(SUM(o."TotalAmount"), 0) FROM orders o WHERE o."StoreId" = @StoreId AND o."CreatedAt" >= NOW() - INTERVAL '30 days' AND o."IsDeleted" = false) as RevenueThisMonth
            FROM stores s
            WHERE s."Id" = @StoreId
            """;

        return await connection.QuerySingleOrDefaultAsync<StoreAnalyticsDto>(sql, new { StoreId = storeId })
               ?? new StoreAnalyticsDto();
    }

    private static string GenerateSlug(string name)
    {
        return name.ToLower()
            .Replace(" ", "-")
            .Replace("&", "and")
            .Replace("'", "")
            .Replace("\"", "");
    }
}

// DTOs
public record StoreDto
{
    public Guid Id { get; init; }
    public Guid OwnerId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? ShortDescription { get; init; }
    public string? LogoUrl { get; init; }
    public string? BannerUrl { get; init; }
    public int Status { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Website { get; init; }
    public string? Address { get; init; }
    public string? City { get; init; }
    public string? State { get; init; }
    public string? Country { get; init; }
    public string? PostalCode { get; init; }
    public decimal CommissionRate { get; init; }
    public decimal Rating { get; init; }
    public int TotalReviews { get; init; }
    public int TotalProducts { get; init; }
    public int TotalOrders { get; init; }
    public decimal TotalSales { get; init; }
    public bool IsVerified { get; init; }
    public bool IsFeatured { get; init; }
    public string? SocialLinks { get; init; }
    public string? BusinessHours { get; init; }
    public string? ShippingPolicy { get; init; }
    public string? ReturnPolicy { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? OwnerName { get; init; }
    public string? OwnerAvatarUrl { get; init; }
}

public record StoreListDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? LogoUrl { get; init; }
    public string? ShortDescription { get; init; }
    public int Status { get; init; }
    public decimal Rating { get; init; }
    public int TotalReviews { get; init; }
    public int TotalProducts { get; init; }
    public int TotalOrders { get; init; }
    public bool IsVerified { get; init; }
    public bool IsFeatured { get; init; }
    public string? City { get; init; }
    public string? Country { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? OwnerName { get; init; }
}

public record CreateStoreDto(
    string Name,
    string? Description = null,
    string? ShortDescription = null,
    string? LogoUrl = null,
    string? BannerUrl = null,
    string? Email = null,
    string? Phone = null,
    string? Website = null,
    string? Address = null,
    string? City = null,
    string? State = null,
    string? Country = null,
    string? PostalCode = null,
    string? ShippingPolicy = null,
    string? ReturnPolicy = null);

public record UpdateStoreDto(
    string? Name = null,
    string? Description = null,
    string? ShortDescription = null,
    string? LogoUrl = null,
    string? BannerUrl = null,
    string? Email = null,
    string? Phone = null,
    string? Website = null,
    string? Address = null,
    string? City = null,
    string? Country = null,
    string? ShippingPolicy = null,
    string? ReturnPolicy = null);

public record StoreEmployeeDto
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

public record StoreAnalyticsDto
{
    public int TotalProducts { get; init; }
    public int TotalOrders { get; init; }
    public decimal TotalSales { get; init; }
    public int TotalReviews { get; init; }
    public decimal AverageRating { get; init; }
    public int PendingOrders { get; init; }
    public int OrdersThisMonth { get; init; }
    public decimal RevenueThisMonth { get; init; }
}
