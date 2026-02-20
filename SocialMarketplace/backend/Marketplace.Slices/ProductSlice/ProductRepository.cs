using Dapper;
using Marketplace.Core.Infrastructure;
using Marketplace.Database.Enums;

namespace Marketplace.Slices.ProductSlice;

public interface IProductRepository
{
    Task<ProductDto?> GetByIdAsync(Guid id);
    Task<ProductDto?> GetBySlugAsync(string slug);
    Task<(IEnumerable<ProductListDto> Products, int TotalCount)> GetAllAsync(ProductQueryParams query);
    Task<IEnumerable<ProductListDto>> GetByStoreIdAsync(Guid storeId, int page, int pageSize);
    Task<Guid> CreateAsync(CreateProductDto dto, Guid storeId);
    Task<bool> UpdateAsync(Guid id, UpdateProductDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<ProductImageDto>> GetImagesAsync(Guid productId);
    Task<IEnumerable<ProductVariantDto>> GetVariantsAsync(Guid productId);
    Task<IEnumerable<ProductReviewDto>> GetReviewsAsync(Guid productId, int page, int pageSize);
}

public class ProductRepository : IProductRepository
{
    private readonly IConnectionFactory _connectionFactory;

    public ProductRepository(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();

        const string sql = """
            SELECT p."Id", p."StoreId", p."CategoryId", p."Name", p."Slug",
                   p."Description", p."ShortDescription", p."Sku", p."Barcode",
                   p."Price", p."CompareAtPrice", p."Currency", p."Status",
                   p."StockQuantity", p."TrackInventory",
                   p."Tags", p."Rating", p."TotalReviews", p."TotalSold",
                   p."ViewCount", p."IsFeatured", p."IsDigital",
                   p."SeoTitle", p."SeoDescription",
                   p."Attributes", p."PublishedAt", p."CreatedAt",
                   s."Name" as StoreName, s."Slug" as StoreSlug,
                   c."Name" as CategoryName
            FROM products p
            JOIN stores s ON p."StoreId" = s."Id"
            JOIN "Categories" c ON p."CategoryId" = c."Id"
            WHERE p."Id" = @Id AND p."IsDeleted" = false
            """;

        return await connection.QuerySingleOrDefaultAsync<ProductDto>(sql, new { Id = id });
    }

    public async Task<ProductDto?> GetBySlugAsync(string slug)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();

        const string sql = """
            SELECT p."Id", p."StoreId", p."CategoryId", p."Name", p."Slug",
                   p."Description", p."ShortDescription", p."Sku",
                   p."Price", p."CompareAtPrice", p."Currency", p."Status",
                   p."StockQuantity", p."Tags", p."Rating",
                   p."TotalReviews", p."TotalSold",
                   p."IsFeatured", p."IsDigital",
                   p."CreatedAt",
                   s."Name" as StoreName, s."Slug" as StoreSlug,
                   c."Name" as CategoryName
            FROM products p
            JOIN stores s ON p."StoreId" = s."Id"
            JOIN "Categories" c ON p."CategoryId" = c."Id"
            WHERE p."Slug" = @Slug AND p."IsDeleted" = false
            """;

        return await connection.QuerySingleOrDefaultAsync<ProductDto>(sql, new { Slug = slug });
    }

    public async Task<(IEnumerable<ProductListDto> Products, int TotalCount)> GetAllAsync(ProductQueryParams query)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();

        var whereClause = "WHERE p.\"IsDeleted\" = false AND p.\"Status\" = @ActiveStatus";
        var parameters = new DynamicParameters();
        parameters.Add("ActiveStatus", (int)ProductStatus.Active);

        if (!string.IsNullOrEmpty(query.Search))
        {
            whereClause += " AND (p.\"Name\" ILIKE @Search OR p.\"Description\" ILIKE @Search OR p.\"Tags\" ILIKE @Search)";
            parameters.Add("Search", $"%{query.Search}%");
        }
        if (query.CategoryId.HasValue)
        {
            whereClause += " AND p.\"CategoryId\" = @CategoryId";
            parameters.Add("CategoryId", query.CategoryId);
        }
        if (query.StoreId.HasValue)
        {
            whereClause += " AND p.\"StoreId\" = @StoreId";
            parameters.Add("StoreId", query.StoreId);
        }
        if (query.MinPrice.HasValue)
        {
            whereClause += " AND p.\"Price\" >= @MinPrice";
            parameters.Add("MinPrice", query.MinPrice);
        }
        if (query.MaxPrice.HasValue)
        {
            whereClause += " AND p.\"Price\" <= @MaxPrice";
            parameters.Add("MaxPrice", query.MaxPrice);
        }

        var countSql = $"SELECT COUNT(*) FROM products p {whereClause}";
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);

        var orderBy = query.SortBy?.ToLower() switch
        {
            "price_asc" => "p.\"Price\" ASC",
            "price_desc" => "p.\"Price\" DESC",
            "rating" => "p.\"Rating\" DESC",
            "newest" => "p.\"CreatedAt\" DESC",
            "popular" => "p.\"TotalSold\" DESC",
            _ => "p.\"IsFeatured\" DESC, p.\"CreatedAt\" DESC"
        };

        parameters.Add("PageSize", query.PageSize);
        parameters.Add("Offset", (query.Page - 1) * query.PageSize);

        var sql = $"""
            SELECT p."Id", p."Name", p."Slug", p."Price", p."CompareAtPrice", p."Currency",
                   p."Rating", p."TotalReviews", p."TotalSold",
                   p."IsFeatured", p."StockQuantity",
                   p."CreatedAt",
                   s."Name" as StoreName, s."Slug" as StoreSlug,
                   c."Name" as CategoryName,
                   (SELECT pi."Url" FROM product_images pi WHERE pi."ProductId" = p."Id" AND pi."IsPrimary" = true AND pi."IsDeleted" = false LIMIT 1) as ImageUrl
            FROM products p
            JOIN stores s ON p."StoreId" = s."Id"
            JOIN "Categories" c ON p."CategoryId" = c."Id"
            {whereClause}
            ORDER BY {orderBy}
            LIMIT @PageSize OFFSET @Offset
            """;

        var products = await connection.QueryAsync<ProductListDto>(sql, parameters);
        return (products, totalCount);
    }

    public async Task<IEnumerable<ProductListDto>> GetByStoreIdAsync(Guid storeId, int page, int pageSize)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();

        const string sql = """
            SELECT p."Id", p."Name", p."Slug", p."Price", p."CompareAtPrice", p."Currency",
                   p."Rating", p."TotalReviews", p."TotalSold",
                   p."Status", p."StockQuantity", p."CreatedAt",
                   c."Name" as CategoryName,
                   (SELECT pi."Url" FROM product_images pi WHERE pi."ProductId" = p."Id" AND pi."IsPrimary" = true LIMIT 1) as ImageUrl
            FROM products p
            JOIN "Categories" c ON p."CategoryId" = c."Id"
            WHERE p."StoreId" = @StoreId AND p."IsDeleted" = false
            ORDER BY p."CreatedAt" DESC
            LIMIT @PageSize OFFSET @Offset
            """;

        return await connection.QueryAsync<ProductListDto>(sql, new
        {
            StoreId = storeId,
            PageSize = pageSize,
            Offset = (page - 1) * pageSize
        });
    }

    public async Task<Guid> CreateAsync(CreateProductDto dto, Guid storeId)
    {
        using var connection = await _connectionFactory.CreateWriteConnectionAsync();

        var id = Guid.NewGuid();
        var slug = dto.Name.ToLower().Replace(" ", "-").Replace("'", "").Replace("\"", "");

        const string sql = """
            INSERT INTO products ("Id", "StoreId", "CategoryId", "Name", "Slug", "Description", "ShortDescription",
                                 "Sku", "Price", "CompareAtPrice", "Currency", "Status", "StockQuantity",
                                 "TrackInventory", "AllowBackorder", "Tags",
                                 "Rating", "TotalReviews", "TotalSold", "ViewCount",
                                 "IsFeatured", "IsDigital", "CreatedAt", "IsDeleted")
            VALUES (@Id, @StoreId, @CategoryId, @Name, @Slug, @Description, @ShortDescription,
                   @Sku, @Price, @CompareAtPrice, @Currency, @Status, @StockQuantity,
                   @TrackInventory, false, @Tags,
                   0, 0, 0, 0,
                   false, @IsDigital, NOW(), false)
            RETURNING "Id"
            """;

        return await connection.ExecuteScalarAsync<Guid>(sql, new
        {
            Id = id,
            StoreId = storeId,
            dto.CategoryId,
            dto.Name,
            Slug = slug,
            dto.Description,
            dto.ShortDescription,
            dto.Sku,
            dto.Price,
            dto.CompareAtPrice,
            Currency = dto.Currency ?? "USD",
            Status = (int)ProductStatus.Draft,
            StockQuantity = dto.StockQuantity ?? 0,
            TrackInventory = dto.TrackInventory ?? true,
            dto.Tags,
            IsDigital = dto.IsDigital ?? false
        });
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateProductDto dto)
    {
        using var connection = await _connectionFactory.CreateWriteConnectionAsync();

        var updates = new List<string>();
        var parameters = new DynamicParameters();
        parameters.Add("Id", id);

        if (dto.Name != null) { updates.Add("\"Name\" = @Name"); parameters.Add("Name", dto.Name); }
        if (dto.Description != null) { updates.Add("\"Description\" = @Description"); parameters.Add("Description", dto.Description); }
        if (dto.Price.HasValue) { updates.Add("\"Price\" = @Price"); parameters.Add("Price", dto.Price); }
        if (dto.CompareAtPrice.HasValue) { updates.Add("\"CompareAtPrice\" = @CompareAtPrice"); parameters.Add("CompareAtPrice", dto.CompareAtPrice); }
        if (dto.StockQuantity.HasValue) { updates.Add("\"StockQuantity\" = @StockQuantity"); parameters.Add("StockQuantity", dto.StockQuantity); }
        if (dto.Status.HasValue) { updates.Add("\"Status\" = @Status"); parameters.Add("Status", dto.Status); }
        if (dto.Tags != null) { updates.Add("\"Tags\" = @Tags"); parameters.Add("Tags", dto.Tags); }
        if (dto.CategoryId.HasValue) { updates.Add("\"CategoryId\" = @CategoryId"); parameters.Add("CategoryId", dto.CategoryId); }

        if (updates.Count == 0) return true;
        updates.Add("\"UpdatedAt\" = NOW()");

        var sql = $"UPDATE products SET {string.Join(", ", updates)} WHERE \"Id\" = @Id AND \"IsDeleted\" = false";
        return await connection.ExecuteAsync(sql, parameters) > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        using var connection = await _connectionFactory.CreateWriteConnectionAsync();
        return await connection.ExecuteAsync(
            "UPDATE products SET \"IsDeleted\" = true, \"DeletedAt\" = NOW() WHERE \"Id\" = @Id", new { Id = id }) > 0;
    }

    public async Task<IEnumerable<ProductImageDto>> GetImagesAsync(Guid productId)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();
        return await connection.QueryAsync<ProductImageDto>(
            """
            SELECT "Id", "Url", "ThumbnailUrl", "AltText", "SortOrder", "IsPrimary"
            FROM product_images WHERE "ProductId" = @ProductId AND "IsDeleted" = false ORDER BY "SortOrder"
            """, new { ProductId = productId });
    }

    public async Task<IEnumerable<ProductVariantDto>> GetVariantsAsync(Guid productId)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();
        return await connection.QueryAsync<ProductVariantDto>(
            """
            SELECT "Id", "Name", "Sku", "Price", "CompareAtPrice", "StockQuantity",
                   "Status", "ImageUrl", "Attributes"
            FROM product_variants WHERE "ProductId" = @ProductId AND "IsDeleted" = false ORDER BY "SortOrder"
            """, new { ProductId = productId });
    }

    public async Task<IEnumerable<ProductReviewDto>> GetReviewsAsync(Guid productId, int page, int pageSize)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();
        return await connection.QueryAsync<ProductReviewDto>(
            """
            SELECT r."Id", r."Rating", r."Title", r."Content", r."IsVerifiedPurchase",
                   r."HelpfulCount", r."CreatedAt",
                   u."FirstName" || ' ' || u."LastName" as ReviewerName, u."AvatarUrl" as ReviewerAvatarUrl
            FROM reviews r
            JOIN users u ON r."ReviewerId" = u."Id"
            WHERE r."ProductId" = @ProductId AND r."IsDeleted" = false
            ORDER BY r."CreatedAt" DESC
            LIMIT @PageSize OFFSET @Offset
            """, new { ProductId = productId, PageSize = pageSize, Offset = (page - 1) * pageSize });
    }
}

// DTOs
public record ProductDto
{
    public Guid Id { get; init; }
    public Guid StoreId { get; init; }
    public Guid CategoryId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? ShortDescription { get; init; }
    public string? Sku { get; init; }
    public string? Barcode { get; init; }
    public decimal Price { get; init; }
    public decimal? CompareAtPrice { get; init; }
    public string Currency { get; init; } = "USD";
    public int Status { get; init; }
    public int StockQuantity { get; init; }
    public bool TrackInventory { get; init; }
    public string? Tags { get; init; }
    public decimal Rating { get; init; }
    public int TotalReviews { get; init; }
    public int TotalSold { get; init; }
    public int ViewCount { get; init; }
    public bool IsFeatured { get; init; }
    public bool IsDigital { get; init; }
    public string? SeoTitle { get; init; }
    public string? SeoDescription { get; init; }
    public string? Attributes { get; init; }
    public DateTime? PublishedAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? StoreName { get; init; }
    public string? StoreSlug { get; init; }
    public string? CategoryName { get; init; }
}

public record ProductListDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public decimal? CompareAtPrice { get; init; }
    public string Currency { get; init; } = "USD";
    public decimal Rating { get; init; }
    public int TotalReviews { get; init; }
    public int TotalSold { get; init; }
    public bool IsFeatured { get; init; }
    public int StockQuantity { get; init; }
    public int Status { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? StoreName { get; init; }
    public string? StoreSlug { get; init; }
    public string? CategoryName { get; init; }
    public string? ImageUrl { get; init; }
}

public record CreateProductDto(
    Guid CategoryId,
    string Name,
    string? Description = null,
    string? ShortDescription = null,
    string? Sku = null,
    decimal Price = 0,
    decimal? CompareAtPrice = null,
    string? Currency = "USD",
    int? StockQuantity = 0,
    bool? TrackInventory = true,
    string? Tags = null,
    bool? IsDigital = false);

public record UpdateProductDto(
    string? Name = null,
    string? Description = null,
    decimal? Price = null,
    decimal? CompareAtPrice = null,
    int? StockQuantity = null,
    int? Status = null,
    string? Tags = null,
    Guid? CategoryId = null);

public record ProductQueryParams
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? Search { get; init; }
    public Guid? CategoryId { get; init; }
    public Guid? StoreId { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public string? SortBy { get; init; }
}

public record ProductImageDto
{
    public Guid Id { get; init; }
    public string Url { get; init; } = string.Empty;
    public string? ThumbnailUrl { get; init; }
    public string? AltText { get; init; }
    public int SortOrder { get; init; }
    public bool IsPrimary { get; init; }
}

public record ProductVariantDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Sku { get; init; }
    public decimal Price { get; init; }
    public decimal? CompareAtPrice { get; init; }
    public int StockQuantity { get; init; }
    public int Status { get; init; }
    public string? ImageUrl { get; init; }
    public string? Attributes { get; init; }
}

public record ProductReviewDto
{
    public Guid Id { get; init; }
    public int Rating { get; init; }
    public string? Title { get; init; }
    public string? Content { get; init; }
    public bool IsVerifiedPurchase { get; init; }
    public int HelpfulCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? ReviewerName { get; init; }
    public string? ReviewerAvatarUrl { get; init; }
}
