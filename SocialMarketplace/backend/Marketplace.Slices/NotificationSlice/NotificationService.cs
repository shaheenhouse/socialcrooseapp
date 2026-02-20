using Dapper;
using Microsoft.Extensions.Logging;
using Marketplace.Core.Infrastructure;

namespace Marketplace.Slices.NotificationSlice;

public interface INotificationRepository
{
    Task<(IEnumerable<NotificationDto> Notifications, int TotalCount)> GetByUserIdAsync(Guid userId, int page, int pageSize, bool? unreadOnly = null);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task<bool> MarkAsReadAsync(Guid id, Guid userId);
    Task<int> MarkAllAsReadAsync(Guid userId);
    Task<Guid> CreateAsync(Guid userId, string type, string title, string message, string? actionUrl = null, string? referenceId = null);
}

public interface INotificationService
{
    Task<(IEnumerable<NotificationDto> Notifications, int TotalCount)> GetMyNotificationsAsync(Guid userId, int page, int pageSize, bool? unreadOnly = null);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task<bool> MarkAsReadAsync(Guid id, Guid userId);
    Task<int> MarkAllAsReadAsync(Guid userId);
}

public class NotificationRepository : INotificationRepository
{
    private readonly IConnectionFactory _connectionFactory;

    public NotificationRepository(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<(IEnumerable<NotificationDto> Notifications, int TotalCount)> GetByUserIdAsync(Guid userId, int page, int pageSize, bool? unreadOnly = null)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();

        var whereClause = @"WHERE ""UserId"" = @UserId AND ""IsDeleted"" = false";
        if (unreadOnly == true) whereClause += @" AND ""IsRead"" = false";

        var totalCount = await connection.ExecuteScalarAsync<int>(
            $"""SELECT COUNT(*) FROM "Notifications" {whereClause}""", new { UserId = userId });

        var notifications = await connection.QueryAsync<NotificationDto>($"""
            SELECT id, type, title, message, "ImageUrl", "ActionUrl",
                   "ActionType", "ReferenceId", "ReferenceType",
                   "IsRead", priority, "CreatedAt"
            FROM "Notifications"
            {whereClause}
            ORDER BY "CreatedAt" DESC
            LIMIT @PageSize OFFSET @Offset
            """, new { UserId = userId, PageSize = pageSize, Offset = (page - 1) * pageSize });

        return (notifications, totalCount);
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();
        return await connection.ExecuteScalarAsync<int>(
            """SELECT COUNT(*) FROM "Notifications" WHERE "UserId" = @UserId AND "IsRead" = false AND "IsDeleted" = false""",
            new { UserId = userId });
    }

    public async Task<bool> MarkAsReadAsync(Guid id, Guid userId)
    {
        using var connection = await _connectionFactory.CreateWriteConnectionAsync();
        return await connection.ExecuteAsync(
            """UPDATE "Notifications" SET "IsRead" = true, "UpdatedAt" = NOW() WHERE id = @Id AND "UserId" = @UserId""",
            new { Id = id, UserId = userId }) > 0;
    }

    public async Task<int> MarkAllAsReadAsync(Guid userId)
    {
        using var connection = await _connectionFactory.CreateWriteConnectionAsync();
        return await connection.ExecuteAsync(
            """UPDATE "Notifications" SET "IsRead" = true, "UpdatedAt" = NOW() WHERE "UserId" = @UserId AND "IsRead" = false""",
            new { UserId = userId });
    }

    public async Task<Guid> CreateAsync(Guid userId, string type, string title, string message, string? actionUrl = null, string? referenceId = null)
    {
        using var connection = await _connectionFactory.CreateWriteConnectionAsync();
        var id = Guid.NewGuid();
        await connection.ExecuteAsync("""
            INSERT INTO "Notifications" (id, "UserId", type, title, message, "ActionUrl", "ReferenceId",
                                        "IsRead", "IsSent", priority, "CreatedAt", "IsDeleted")
            VALUES (@Id, @UserId, @Type::integer, @Title, @Message, @ActionUrl, @ReferenceId,
                   false, false, 2, NOW(), false)
            """, new { Id = id, UserId = userId, Type = type, Title = title, Message = message, ActionUrl = actionUrl, ReferenceId = referenceId });
        return id;
    }
}

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repository;

    public NotificationService(INotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<(IEnumerable<NotificationDto> Notifications, int TotalCount)> GetMyNotificationsAsync(Guid userId, int page, int pageSize, bool? unreadOnly = null)
        => await _repository.GetByUserIdAsync(userId, page, pageSize, unreadOnly);

    public async Task<int> GetUnreadCountAsync(Guid userId) => await _repository.GetUnreadCountAsync(userId);
    public async Task<bool> MarkAsReadAsync(Guid id, Guid userId) => await _repository.MarkAsReadAsync(id, userId);
    public async Task<int> MarkAllAsReadAsync(Guid userId) => await _repository.MarkAllAsReadAsync(userId);
}

// DTOs
public record NotificationDto
{
    public Guid Id { get; init; }
    public int Type { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public string? ActionUrl { get; init; }
    public string? ActionType { get; init; }
    public string? ReferenceId { get; init; }
    public string? ReferenceType { get; init; }
    public bool IsRead { get; init; }
    public int Priority { get; init; }
    public DateTime CreatedAt { get; init; }
}
