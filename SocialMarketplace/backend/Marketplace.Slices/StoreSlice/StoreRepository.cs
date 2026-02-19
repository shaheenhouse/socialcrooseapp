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
            SELECT s.id, s.owner_id as OwnerId, s.name, s.slug, s.description, s.short_description as ShortDescription,
                   s.logo_url as LogoUrl, s.banner_url as BannerUrl, s.status, s.email, s.phone, s.website,
                   s.address, s.city, s.state, s.country, s.postal_code as PostalCode,
                   s.commission_rate as CommissionRate, s.rating, s.total_reviews as TotalReviews,
                   s.total_products as TotalProducts, s.total_orders as TotalOrders, s.total_sales as TotalSales,
                   s.is_verified as IsVerified, s.is_featured as IsFeatured,
                   s.social_links as SocialLinks, s.business_hours as BusinessHours,
                   s.shipping_policy as ShippingPolicy, s.return_policy as ReturnPolicy,
                   s.created_at as CreatedAt,
                   u.first_name || ' ' || u.last_name as OwnerName, u.avatar_url as OwnerAvatarUrl
            FROM stores s
            JOIN users u ON s.owner_id = u.id
            WHERE s.id = @Id AND s.is_deleted = false
            """;

        return await connection.QuerySingleOrDefaultAsync<StoreDto>(sql, new { Id = id });
    }

    public async Task<StoreDto?> GetBySlugAsync(string slug)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();

        const string sql = """
            SELECT s.id, s.owner_id as OwnerId, s.name, s.slug, s.description, s.short_description as ShortDescription,
                   s.logo_url as LogoUrl, s.banner_url as BannerUrl, s.status, s.email, s.phone, s.website,
                   s.address, s.city, s.state, s.country, s.postal_code as PostalCode,
                   s.commission_rate as CommissionRate, s.rating, s.total_reviews as TotalReviews,
                   s.total_products as TotalProducts, s.total_orders as TotalOrders, s.total_sales as TotalSales,
                   s.is_verified as IsVerified, s.is_featured as IsFeatured,
                   s.social_links as SocialLinks, s.business_hours as BusinessHours,
                   s.shipping_policy as ShippingPolicy, s.return_policy as ReturnPolicy,
                   s.created_at as CreatedAt,
                   u.first_name || ' ' || u.last_name as OwnerName, u.avatar_url as OwnerAvatarUrl
            FROM stores s
            JOIN users u ON s.owner_id = u.id
            WHERE s.slug = @Slug AND s.is_deleted = false
            """;

        return await connection.QuerySingleOrDefaultAsync<StoreDto>(sql, new { Slug = slug });
    }

    public async Task<StoreDto?> GetByOwnerIdAsync(Guid ownerId)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();

        const string sql = """
            SELECT s.id, s.owner_id as OwnerId, s.name, s.slug, s.description,
                   s.logo_url as LogoUrl, s.banner_url as BannerUrl, s.status,
                   s.rating, s.total_reviews as TotalReviews, s.total_products as TotalProducts,
                   s.total_orders as TotalOrders, s.total_sales as TotalSales,
                   s.is_verified as IsVerified, s.created_at as CreatedAt
            FROM stores s
            WHERE s.owner_id = @OwnerId AND s.is_deleted = false
            LIMIT 1
            """;

        return await connection.QuerySingleOrDefaultAsync<StoreDto>(sql, new { OwnerId = ownerId });
    }

    public async Task<(IEnumerable<StoreListDto> Stores, int TotalCount)> GetAllAsync(int page, int pageSize, string? search = null, string? status = null)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();

        var whereClause = "WHERE s.is_deleted = false";
        if (!string.IsNullOrEmpty(search))
            whereClause += " AND (s.name ILIKE @Search OR s.description ILIKE @Search)";
        if (!string.IsNullOrEmpty(status))
            whereClause += " AND s.status = @Status::integer";

        var countSql = $"SELECT COUNT(*) FROM stores s {whereClause}";
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, new { Search = $"%{search}%", Status = status });

        var sql = $"""
            SELECT s.id, s.name, s.slug, s.logo_url as LogoUrl, s.short_description as ShortDescription,
                   s.status, s.rating, s.total_reviews as TotalReviews, s.total_products as TotalProducts,
                   s.total_orders as TotalOrders, s.is_verified as IsVerified, s.is_featured as IsFeatured,
                   s.city, s.country, s.created_at as CreatedAt,
                   u.first_name || ' ' || u.last_name as OwnerName
            FROM stores s
            JOIN users u ON s.owner_id = u.id
            {whereClause}
            ORDER BY s.is_featured DESC, s.rating DESC, s.created_at DESC
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
            INSERT INTO stores (id, owner_id, name, slug, description, short_description, logo_url, banner_url,
                               email, phone, website, address, city, state, country, postal_code,
                               status, shipping_policy, return_policy, created_at, is_deleted)
            VALUES (@Id, @OwnerId, @Name, @Slug, @Description, @ShortDescription, @LogoUrl, @BannerUrl,
                   @Email, @Phone, @Website, @Address, @City, @State, @Country, @PostalCode,
                   @Status, @ShippingPolicy, @ReturnPolicy, NOW(), false)
            RETURNING id
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

        if (dto.Name != null) { updates.Add("name = @Name"); parameters.Add("Name", dto.Name); }
        if (dto.Description != null) { updates.Add("description = @Description"); parameters.Add("Description", dto.Description); }
        if (dto.ShortDescription != null) { updates.Add("short_description = @ShortDescription"); parameters.Add("ShortDescription", dto.ShortDescription); }
        if (dto.LogoUrl != null) { updates.Add("logo_url = @LogoUrl"); parameters.Add("LogoUrl", dto.LogoUrl); }
        if (dto.BannerUrl != null) { updates.Add("banner_url = @BannerUrl"); parameters.Add("BannerUrl", dto.BannerUrl); }
        if (dto.Email != null) { updates.Add("email = @Email"); parameters.Add("Email", dto.Email); }
        if (dto.Phone != null) { updates.Add("phone = @Phone"); parameters.Add("Phone", dto.Phone); }
        if (dto.Website != null) { updates.Add("website = @Website"); parameters.Add("Website", dto.Website); }
        if (dto.Address != null) { updates.Add("address = @Address"); parameters.Add("Address", dto.Address); }
        if (dto.City != null) { updates.Add("city = @City"); parameters.Add("City", dto.City); }
        if (dto.Country != null) { updates.Add("country = @Country"); parameters.Add("Country", dto.Country); }
        if (dto.ShippingPolicy != null) { updates.Add("shipping_policy = @ShippingPolicy"); parameters.Add("ShippingPolicy", dto.ShippingPolicy); }
        if (dto.ReturnPolicy != null) { updates.Add("return_policy = @ReturnPolicy"); parameters.Add("ReturnPolicy", dto.ReturnPolicy); }

        if (updates.Count == 0) return true;
        updates.Add("updated_at = NOW()");

        var sql = $"UPDATE stores SET {string.Join(", ", updates)} WHERE id = @Id AND is_deleted = false";
        return await connection.ExecuteAsync(sql, parameters) > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        using var connection = await _connectionFactory.CreateWriteConnectionAsync();
        return await connection.ExecuteAsync(
            "UPDATE stores SET is_deleted = true, deleted_at = NOW() WHERE id = @Id", new { Id = id }) > 0;
    }

    public async Task<IEnumerable<StoreEmployeeDto>> GetEmployeesAsync(Guid storeId)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();

        const string sql = """
            SELECT se.id, se.user_id as UserId, se.title, se.department,
                   se.is_active as IsActive, se.joined_at as JoinedAt,
                   u.first_name as FirstName, u.last_name as LastName,
                   u.avatar_url as AvatarUrl, u.email
            FROM store_employees se
            JOIN users u ON se.user_id = u.id
            WHERE se.store_id = @StoreId AND se.is_active = true
            ORDER BY se.joined_at
            """;

        return await connection.QueryAsync<StoreEmployeeDto>(sql, new { StoreId = storeId });
    }

    public async Task<StoreAnalyticsDto> GetAnalyticsAsync(Guid storeId)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();

        const string sql = """
            SELECT 
                s.total_products as TotalProducts,
                s.total_orders as TotalOrders,
                s.total_sales as TotalSales,
                s.total_reviews as TotalReviews,
                s.rating as AverageRating,
                (SELECT COUNT(*) FROM orders o WHERE o.store_id = @StoreId AND o.status = 0 AND o.is_deleted = false) as PendingOrders,
                (SELECT COUNT(*) FROM orders o WHERE o.store_id = @StoreId AND o.created_at >= NOW() - INTERVAL '30 days' AND o.is_deleted = false) as OrdersThisMonth,
                (SELECT COALESCE(SUM(o.total_amount), 0) FROM orders o WHERE o.store_id = @StoreId AND o.created_at >= NOW() - INTERVAL '30 days' AND o.is_deleted = false) as RevenueThisMonth
            FROM stores s
            WHERE s.id = @StoreId
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
