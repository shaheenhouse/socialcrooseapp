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
            SELECT p.id, p.store_id as StoreId, p.category_id as CategoryId, p.name, p.slug,
                   p.description, p.short_description as ShortDescription, p.sku, p.barcode,
                   p.price, p.compare_at_price as CompareAtPrice, p.currency, p.status,
                   p.stock_quantity as StockQuantity, p.track_inventory as TrackInventory,
                   p.tags, p.rating, p.total_reviews as TotalReviews, p.total_sold as TotalSold,
                   p.view_count as ViewCount, p.is_featured as IsFeatured, p.is_digital as IsDigital,
                   p.seo_title as SeoTitle, p.seo_description as SeoDescription,
                   p.attributes, p.published_at as PublishedAt, p.created_at as CreatedAt,
                   s.name as StoreName, s.slug as StoreSlug,
                   c.name as CategoryName
            FROM products p
            JOIN stores s ON p.store_id = s.id
            JOIN categories c ON p.category_id = c.id
            WHERE p.id = @Id AND p.is_deleted = false
            """;

        return await connection.QuerySingleOrDefaultAsync<ProductDto>(sql, new { Id = id });
    }

    public async Task<ProductDto?> GetBySlugAsync(string slug)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();

        const string sql = """
            SELECT p.id, p.store_id as StoreId, p.category_id as CategoryId, p.name, p.slug,
                   p.description, p.short_description as ShortDescription, p.sku,
                   p.price, p.compare_at_price as CompareAtPrice, p.currency, p.status,
                   p.stock_quantity as StockQuantity, p.tags, p.rating,
                   p.total_reviews as TotalReviews, p.total_sold as TotalSold,
                   p.is_featured as IsFeatured, p.is_digital as IsDigital,
                   p.created_at as CreatedAt,
                   s.name as StoreName, s.slug as StoreSlug,
                   c.name as CategoryName
            FROM products p
            JOIN stores s ON p.store_id = s.id
            JOIN categories c ON p.category_id = c.id
            WHERE p.slug = @Slug AND p.is_deleted = false
            """;

        return await connection.QuerySingleOrDefaultAsync<ProductDto>(sql, new { Slug = slug });
    }

    public async Task<(IEnumerable<ProductListDto> Products, int TotalCount)> GetAllAsync(ProductQueryParams query)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();

        var whereClause = "WHERE p.is_deleted = false AND p.status = @ActiveStatus";
        var parameters = new DynamicParameters();
        parameters.Add("ActiveStatus", (int)ProductStatus.Active);

        if (!string.IsNullOrEmpty(query.Search))
        {
            whereClause += " AND (p.name ILIKE @Search OR p.description ILIKE @Search OR p.tags ILIKE @Search)";
            parameters.Add("Search", $"%{query.Search}%");
        }
        if (query.CategoryId.HasValue)
        {
            whereClause += " AND p.category_id = @CategoryId";
            parameters.Add("CategoryId", query.CategoryId);
        }
        if (query.StoreId.HasValue)
        {
            whereClause += " AND p.store_id = @StoreId";
            parameters.Add("StoreId", query.StoreId);
        }
        if (query.MinPrice.HasValue)
        {
            whereClause += " AND p.price >= @MinPrice";
            parameters.Add("MinPrice", query.MinPrice);
        }
        if (query.MaxPrice.HasValue)
        {
            whereClause += " AND p.price <= @MaxPrice";
            parameters.Add("MaxPrice", query.MaxPrice);
        }

        var countSql = $"SELECT COUNT(*) FROM products p {whereClause}";
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);

        var orderBy = query.SortBy?.ToLower() switch
        {
            "price_asc" => "p.price ASC",
            "price_desc" => "p.price DESC",
            "rating" => "p.rating DESC",
            "newest" => "p.created_at DESC",
            "popular" => "p.total_sold DESC",
            _ => "p.is_featured DESC, p.created_at DESC"
        };

        parameters.Add("PageSize", query.PageSize);
        parameters.Add("Offset", (query.Page - 1) * query.PageSize);

        var sql = $"""
            SELECT p.id, p.name, p.slug, p.price, p.compare_at_price as CompareAtPrice, p.currency,
                   p.rating, p.total_reviews as TotalReviews, p.total_sold as TotalSold,
                   p.is_featured as IsFeatured, p.stock_quantity as StockQuantity,
                   p.created_at as CreatedAt,
                   s.name as StoreName, s.slug as StoreSlug,
                   c.name as CategoryName,
                   (SELECT url FROM product_images pi WHERE pi.product_id = p.id AND pi.is_primary = true AND pi.is_deleted = false LIMIT 1) as ImageUrl
            FROM products p
            JOIN stores s ON p.store_id = s.id
            JOIN categories c ON p.category_id = c.id
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
            SELECT p.id, p.name, p.slug, p.price, p.compare_at_price as CompareAtPrice, p.currency,
                   p.rating, p.total_reviews as TotalReviews, p.total_sold as TotalSold,
                   p.status, p.stock_quantity as StockQuantity, p.created_at as CreatedAt,
                   c.name as CategoryName,
                   (SELECT url FROM product_images pi WHERE pi.product_id = p.id AND pi.is_primary = true LIMIT 1) as ImageUrl
            FROM products p
            JOIN categories c ON p.category_id = c.id
            WHERE p.store_id = @StoreId AND p.is_deleted = false
            ORDER BY p.created_at DESC
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
            INSERT INTO products (id, store_id, category_id, name, slug, description, short_description,
                                 sku, price, compare_at_price, currency, status, stock_quantity,
                                 track_inventory, tags, is_digital, created_at, is_deleted)
            VALUES (@Id, @StoreId, @CategoryId, @Name, @Slug, @Description, @ShortDescription,
                   @Sku, @Price, @CompareAtPrice, @Currency, @Status, @StockQuantity,
                   @TrackInventory, @Tags, @IsDigital, NOW(), false)
            RETURNING id
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

        if (dto.Name != null) { updates.Add("name = @Name"); parameters.Add("Name", dto.Name); }
        if (dto.Description != null) { updates.Add("description = @Description"); parameters.Add("Description", dto.Description); }
        if (dto.Price.HasValue) { updates.Add("price = @Price"); parameters.Add("Price", dto.Price); }
        if (dto.CompareAtPrice.HasValue) { updates.Add("compare_at_price = @CompareAtPrice"); parameters.Add("CompareAtPrice", dto.CompareAtPrice); }
        if (dto.StockQuantity.HasValue) { updates.Add("stock_quantity = @StockQuantity"); parameters.Add("StockQuantity", dto.StockQuantity); }
        if (dto.Status.HasValue) { updates.Add("status = @Status"); parameters.Add("Status", dto.Status); }
        if (dto.Tags != null) { updates.Add("tags = @Tags"); parameters.Add("Tags", dto.Tags); }
        if (dto.CategoryId.HasValue) { updates.Add("category_id = @CategoryId"); parameters.Add("CategoryId", dto.CategoryId); }

        if (updates.Count == 0) return true;
        updates.Add("updated_at = NOW()");

        var sql = $"UPDATE products SET {string.Join(", ", updates)} WHERE id = @Id AND is_deleted = false";
        return await connection.ExecuteAsync(sql, parameters) > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        using var connection = await _connectionFactory.CreateWriteConnectionAsync();
        return await connection.ExecuteAsync(
            "UPDATE products SET is_deleted = true, deleted_at = NOW() WHERE id = @Id", new { Id = id }) > 0;
    }

    public async Task<IEnumerable<ProductImageDto>> GetImagesAsync(Guid productId)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();
        return await connection.QueryAsync<ProductImageDto>(
            """
            SELECT id, url, thumbnail_url as ThumbnailUrl, alt_text as AltText, sort_order as SortOrder, is_primary as IsPrimary
            FROM product_images WHERE product_id = @ProductId AND is_deleted = false ORDER BY sort_order
            """, new { ProductId = productId });
    }

    public async Task<IEnumerable<ProductVariantDto>> GetVariantsAsync(Guid productId)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();
        return await connection.QueryAsync<ProductVariantDto>(
            """
            SELECT id, name, sku, price, compare_at_price as CompareAtPrice, stock_quantity as StockQuantity,
                   status, image_url as ImageUrl, attributes
            FROM product_variants WHERE product_id = @ProductId AND is_deleted = false ORDER BY sort_order
            """, new { ProductId = productId });
    }

    public async Task<IEnumerable<ProductReviewDto>> GetReviewsAsync(Guid productId, int page, int pageSize)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();
        return await connection.QueryAsync<ProductReviewDto>(
            """
            SELECT r.id, r.rating, r.title, r.content, r.is_verified_purchase as IsVerifiedPurchase,
                   r.helpful_count as HelpfulCount, r.created_at as CreatedAt,
                   u.first_name || ' ' || u.last_name as ReviewerName, u.avatar_url as ReviewerAvatarUrl
            FROM reviews r
            JOIN users u ON r.reviewer_id = u.id
            WHERE r.product_id = @ProductId AND r.is_deleted = false
            ORDER BY r.created_at DESC
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
