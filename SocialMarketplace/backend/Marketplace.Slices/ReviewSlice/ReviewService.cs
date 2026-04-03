using Dapper;
using Microsoft.Extensions.Logging;
using Marketplace.Core.Caching;
using Marketplace.Core.Infrastructure;

namespace Marketplace.Slices.ReviewSlice;

public interface IReviewRepository
{
    Task<ReviewDto?> GetByIdAsync(Guid id);
    Task<(IEnumerable<ReviewListDto> Reviews, int TotalCount)> GetByEntityAsync(string entityType, Guid entityId, int page, int pageSize);
    Task<Guid> CreateAsync(CreateReviewDto dto, Guid reviewerId);
    Task<bool> UpdateAsync(Guid id, string content, int rating);
    Task<bool> DeleteAsync(Guid id);
    Task<Guid> CreateResponseAsync(Guid reviewId, Guid responderId, string content);
    Task<ReviewStatsDto> GetStatsAsync(string entityType, Guid entityId);
}

public interface IReviewService
{
    Task<ReviewDto?> GetByIdAsync(Guid id);
    Task<(IEnumerable<ReviewListDto> Reviews, int TotalCount)> GetByEntityAsync(string entityType, Guid entityId, int page, int pageSize);
    Task<Guid> CreateAsync(CreateReviewDto dto, Guid reviewerId);
    Task<bool> UpdateAsync(Guid id, Guid userId, string content, int rating);
    Task<bool> DeleteAsync(Guid id, Guid userId);
    Task<Guid> RespondAsync(Guid reviewId, Guid responderId, string content);
    Task<ReviewStatsDto> GetStatsAsync(string entityType, Guid entityId);
}

public class ReviewRepository : IReviewRepository
{
    private readonly IConnectionFactory _connectionFactory;

    public ReviewRepository(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<ReviewDto?> GetByIdAsync(Guid id)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<ReviewDto>("""
            SELECT r."Id", r."ReviewerId", r."ReviewType",
                   r."Rating", r."Title", r."Content", r."Pros", r."Cons",
                   r."IsVerifiedPurchase",
                   r."HelpfulCount", r."UnhelpfulCount",
                   r."QualityRating", r."CommunicationRating",
                   r."ValueRating", r."DeliveryRating",
                   r."CreatedAt",
                   u."FirstName" || ' ' || u."LastName" AS ReviewerName, u."AvatarUrl" AS ReviewerAvatarUrl,
                   rr."Content" AS ResponseContent, rr."CreatedAt" AS ResponseCreatedAt,
                   ru."FirstName" || ' ' || ru."LastName" AS ResponderName
            FROM reviews r
            INNER JOIN users u ON r."ReviewerId" = u."Id"
            LEFT JOIN review_responses rr ON r."Id" = rr."ReviewId" AND rr."IsDeleted" = false
            LEFT JOIN users ru ON rr."ResponderId" = ru."Id"
            WHERE r."Id" = @Id AND r."IsDeleted" = false
            """, new { Id = id });
    }

    public async Task<(IEnumerable<ReviewListDto> Reviews, int TotalCount)> GetByEntityAsync(string entityType, Guid entityId, int page, int pageSize)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();

        var entityColumn = entityType.ToLower() switch
        {
            "product" => @"""ProductId""",
            "service" => @"""ServiceId""",
            "store" => @"""StoreId""",
            "seller" or "user" => @"""RevieweeId""",
            "project" => @"""ProjectId""",
            _ => @"""ProductId"""
        };

        var countSql = $"""SELECT COUNT(*) FROM reviews WHERE {entityColumn} = @EntityId AND "IsDeleted" = false""";
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, new { EntityId = entityId });

        var sql = $"""
            SELECT r."Id", r."Rating", r."Title", r."Content", r."Pros", r."Cons",
                   r."IsVerifiedPurchase",
                   r."HelpfulCount", r."CreatedAt",
                   u."FirstName" || ' ' || u."LastName" AS ReviewerName, u."AvatarUrl" AS ReviewerAvatarUrl,
                   EXISTS(SELECT 1 FROM review_responses rr WHERE rr."ReviewId" = r."Id" AND rr."IsDeleted" = false) AS HasResponse
            FROM reviews r
            INNER JOIN users u ON r."ReviewerId" = u."Id"
            WHERE r.{entityColumn} = @EntityId AND r."IsDeleted" = false
            ORDER BY r."CreatedAt" DESC
            LIMIT @PageSize OFFSET @Offset
            """;

        var reviews = await connection.QueryAsync<ReviewListDto>(sql, new
        {
            EntityId = entityId,
            PageSize = pageSize,
            Offset = (page - 1) * pageSize
        });

        return (reviews, totalCount);
    }

    public async Task<Guid> CreateAsync(CreateReviewDto dto, Guid reviewerId)
    {
        using var connection = await _connectionFactory.CreateWriteConnectionAsync();
        var id = Guid.NewGuid();

        await connection.ExecuteAsync("""
            INSERT INTO reviews ("Id", "ReviewerId", "RevieweeId", "OrderId", "ProductId", "ServiceId", "StoreId", "ProjectId",
                               "ReviewType", "Rating", "Title", "Content", "Pros", "Cons", "IsVerifiedPurchase",
                               "IsRecommended", "HelpfulCount", "UnhelpfulCount", "ReportCount",
                               "IsFlagged", "IsHidden", "HasResponse",
                               "QualityRating", "CommunicationRating", "ValueRating", "DeliveryRating",
                               "CreatedAt", "IsDeleted")
            VALUES (@Id, @ReviewerId, @RevieweeId, @OrderId, @ProductId, @ServiceId, @StoreId, @ProjectId,
                   @ReviewType, @Rating, @Title, @Content, @Pros, @Cons, @IsVerifiedPurchase,
                   true, 0, 0, 0,
                   false, false, false,
                   @QualityRating, @CommunicationRating, @ValueRating, @DeliveryRating,
                   NOW(), false)
            """, new
        {
            Id = id,
            ReviewerId = reviewerId,
            dto.RevieweeId,
            dto.OrderId,
            dto.ProductId,
            dto.ServiceId,
            dto.StoreId,
            dto.ProjectId,
            ReviewType = dto.ReviewType ?? "Product",
            dto.Rating,
            dto.Title,
            dto.Content,
            dto.Pros,
            dto.Cons,
            IsVerifiedPurchase = dto.IsVerifiedPurchase ?? false,
            dto.QualityRating,
            dto.CommunicationRating,
            dto.ValueRating,
            dto.DeliveryRating
        });

        return id;
    }

    public async Task<bool> UpdateAsync(Guid id, string content, int rating)
    {
        using var connection = await _connectionFactory.CreateWriteConnectionAsync();
        return await connection.ExecuteAsync(
            """UPDATE reviews SET "Content" = @Content, "Rating" = @Rating, "UpdatedAt" = NOW() WHERE "Id" = @Id AND "IsDeleted" = false""",
            new { Id = id, Content = content, Rating = rating }) > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        using var connection = await _connectionFactory.CreateWriteConnectionAsync();
        return await connection.ExecuteAsync(
            """UPDATE reviews SET "IsDeleted" = true, "DeletedAt" = NOW() WHERE "Id" = @Id""", new { Id = id }) > 0;
    }

    public async Task<Guid> CreateResponseAsync(Guid reviewId, Guid responderId, string content)
    {
        using var connection = await _connectionFactory.CreateWriteConnectionAsync();
        var id = Guid.NewGuid();
        await connection.ExecuteAsync("""
            INSERT INTO review_responses ("Id", "ReviewId", "ResponderId", "Content", "IsHidden", "CreatedAt", "IsDeleted")
            VALUES (@Id, @ReviewId, @ResponderId, @Content, false, NOW(), false)
            """, new { Id = id, ReviewId = reviewId, ResponderId = responderId, Content = content });
        return id;
    }

    public async Task<ReviewStatsDto> GetStatsAsync(string entityType, Guid entityId)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();
        var entityColumn = entityType.ToLower() switch
        {
            "product" => @"""ProductId""",
            "service" => @"""ServiceId""",
            "store" => @"""StoreId""",
            "seller" or "user" => @"""RevieweeId""",
            _ => @"""ProductId"""
        };

        return await connection.QuerySingleOrDefaultAsync<ReviewStatsDto>($"""
            SELECT COUNT(*) AS TotalReviews,
                   COALESCE(AVG("Rating"), 0) AS AverageRating,
                   COUNT(CASE WHEN "Rating" = 5 THEN 1 END) AS FiveStarCount,
                   COUNT(CASE WHEN "Rating" = 4 THEN 1 END) AS FourStarCount,
                   COUNT(CASE WHEN "Rating" = 3 THEN 1 END) AS ThreeStarCount,
                   COUNT(CASE WHEN "Rating" = 2 THEN 1 END) AS TwoStarCount,
                   COUNT(CASE WHEN "Rating" = 1 THEN 1 END) AS OneStarCount
            FROM reviews WHERE {entityColumn} = @EntityId AND "IsDeleted" = false
            """, new { EntityId = entityId }) ?? new ReviewStatsDto();
    }
}

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _repository;
    private readonly ILogger<ReviewService> _logger;

    public ReviewService(IReviewRepository repository, ILogger<ReviewService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<ReviewDto?> GetByIdAsync(Guid id) => await _repository.GetByIdAsync(id);

    public async Task<(IEnumerable<ReviewListDto> Reviews, int TotalCount)> GetByEntityAsync(string entityType, Guid entityId, int page, int pageSize)
        => await _repository.GetByEntityAsync(entityType, entityId, page, pageSize);

    public async Task<Guid> CreateAsync(CreateReviewDto dto, Guid reviewerId)
    {
        var id = await _repository.CreateAsync(dto, reviewerId);
        _logger.LogInformation("Review created: {ReviewId} by {ReviewerId}", id, reviewerId);
        return id;
    }

    public async Task<bool> UpdateAsync(Guid id, Guid userId, string content, int rating)
    {
        var review = await _repository.GetByIdAsync(id);
        if (review == null || review.ReviewerId != userId) return false;
        return await _repository.UpdateAsync(id, content, rating);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId)
    {
        var review = await _repository.GetByIdAsync(id);
        if (review == null || review.ReviewerId != userId) return false;
        return await _repository.DeleteAsync(id);
    }

    public async Task<Guid> RespondAsync(Guid reviewId, Guid responderId, string content)
        => await _repository.CreateResponseAsync(reviewId, responderId, content);

    public async Task<ReviewStatsDto> GetStatsAsync(string entityType, Guid entityId)
        => await _repository.GetStatsAsync(entityType, entityId);
}

// DTOs
public record ReviewDto
{
    public Guid Id { get; init; }
    public Guid ReviewerId { get; init; }
    public string ReviewType { get; init; } = "Product";
    public int Rating { get; init; }
    public string? Title { get; init; }
    public string? Content { get; init; }
    public string? Pros { get; init; }
    public string? Cons { get; init; }
    public bool IsVerifiedPurchase { get; init; }
    public int HelpfulCount { get; init; }
    public int UnhelpfulCount { get; init; }
    public int? QualityRating { get; init; }
    public int? CommunicationRating { get; init; }
    public int? ValueRating { get; init; }
    public int? DeliveryRating { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? ReviewerName { get; init; }
    public string? ReviewerAvatarUrl { get; init; }
    public string? ResponseContent { get; init; }
    public DateTime? ResponseCreatedAt { get; init; }
    public string? ResponderName { get; init; }
}

public record ReviewListDto
{
    public Guid Id { get; init; }
    public int Rating { get; init; }
    public string? Title { get; init; }
    public string? Content { get; init; }
    public string? Pros { get; init; }
    public string? Cons { get; init; }
    public bool IsVerifiedPurchase { get; init; }
    public int HelpfulCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? ReviewerName { get; init; }
    public string? ReviewerAvatarUrl { get; init; }
    public bool HasResponse { get; init; }
}

public record CreateReviewDto(
    Guid? RevieweeId = null,
    Guid? OrderId = null,
    Guid? ProductId = null,
    Guid? ServiceId = null,
    Guid? StoreId = null,
    Guid? ProjectId = null,
    string? ReviewType = "Product",
    int Rating = 5,
    string? Title = null,
    string? Content = null,
    string? Pros = null,
    string? Cons = null,
    bool? IsVerifiedPurchase = false,
    int? QualityRating = null,
    int? CommunicationRating = null,
    int? ValueRating = null,
    int? DeliveryRating = null);

public record ReviewStatsDto
{
    public int TotalReviews { get; init; }
    public decimal AverageRating { get; init; }
    public int FiveStarCount { get; init; }
    public int FourStarCount { get; init; }
    public int ThreeStarCount { get; init; }
    public int TwoStarCount { get; init; }
    public int OneStarCount { get; init; }
}
