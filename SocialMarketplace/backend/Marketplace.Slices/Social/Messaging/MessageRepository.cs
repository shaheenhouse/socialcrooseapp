using Dapper;
using Marketplace.Core.Infrastructure;
using Marketplace.Database.Entities.Social;

namespace Marketplace.Slices.Social.Messaging;

public interface IMessageRepository
{
    Task<Message?> GetByIdAsync(Guid id);
    Task<IEnumerable<Message>> GetConversationMessagesAsync(Guid conversationId, int page, int pageSize, Guid? beforeMessageId = null);
    Task<Guid> CreateAsync(Message message);
    Task UpdateAsync(Message message);
    Task SoftDeleteAsync(Guid id);
}

public interface IConversationRepository
{
    Task<Conversation?> GetByIdAsync(Guid id);
    Task<Conversation?> GetDirectConversationAsync(Guid userId1, Guid userId2);
    Task<IEnumerable<ConversationWithParticipant>> GetUserConversationsAsync(Guid userId, int page, int pageSize, bool includeArchived = false);
    Task<Guid> CreateAsync(Conversation conversation);
    Task UpdateLastMessageAsync(Guid conversationId, Guid messageId);
    Task<ConversationParticipant?> GetParticipantAsync(Guid conversationId, Guid userId);
    Task AddParticipantAsync(ConversationParticipant participant);
    Task UpdateParticipantAsync(ConversationParticipant participant);
    Task RemoveParticipantAsync(Guid conversationId, Guid userId);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task MarkAsReadAsync(Guid conversationId, Guid userId, Guid messageId);
}

public class MessageRepository : IMessageRepository
{
    private readonly IConnectionFactory _connectionFactory;

    public MessageRepository(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Message?> GetByIdAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateReadConnection();
        return await connection.QueryFirstOrDefaultAsync<Message>(
            "SELECT * FROM messages WHERE id = @Id AND deleted_at IS NULL",
            new { Id = id });
    }

    public async Task<IEnumerable<Message>> GetConversationMessagesAsync(Guid conversationId, int page, int pageSize, Guid? beforeMessageId = null)
    {
        using var connection = _connectionFactory.CreateReadConnection();
        var offset = (page - 1) * pageSize;
        
        if (beforeMessageId.HasValue)
        {
            return await connection.QueryAsync<Message>(
                @"SELECT * FROM messages 
                  WHERE conversation_id = @ConversationId 
                  AND deleted_at IS NULL
                  AND created_at < (SELECT created_at FROM messages WHERE id = @BeforeId)
                  ORDER BY created_at DESC
                  LIMIT @PageSize",
                new { ConversationId = conversationId, BeforeId = beforeMessageId, PageSize = pageSize });
        }
        
        return await connection.QueryAsync<Message>(
            @"SELECT * FROM messages 
              WHERE conversation_id = @ConversationId AND deleted_at IS NULL
              ORDER BY created_at DESC
              LIMIT @PageSize OFFSET @Offset",
            new { ConversationId = conversationId, PageSize = pageSize, Offset = offset });
    }

    public async Task<Guid> CreateAsync(Message message)
    {
        using var connection = _connectionFactory.CreateWriteConnection();
        var id = Guid.NewGuid();
        await connection.ExecuteAsync(
            @"INSERT INTO messages (id, conversation_id, sender_id, content, type, attachment_url, attachment_name, attachment_size, reply_to_id, is_edited, created_at)
              VALUES (@Id, @ConversationId, @SenderId, @Content, @Type, @AttachmentUrl, @AttachmentName, @AttachmentSize, @ReplyToId, @IsEdited, @CreatedAt)",
            new
            {
                Id = id,
                message.ConversationId,
                message.SenderId,
                message.Content,
                message.Type,
                message.AttachmentUrl,
                message.AttachmentName,
                message.AttachmentSize,
                message.ReplyToId,
                message.IsEdited,
                CreatedAt = DateTime.UtcNow
            });
        return id;
    }

    public async Task UpdateAsync(Message message)
    {
        using var connection = _connectionFactory.CreateWriteConnection();
        await connection.ExecuteAsync(
            @"UPDATE messages 
              SET content = @Content, is_edited = true, edited_at = @EditedAt
              WHERE id = @Id",
            new { message.Id, message.Content, EditedAt = DateTime.UtcNow });
    }

    public async Task SoftDeleteAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateWriteConnection();
        await connection.ExecuteAsync(
            "UPDATE messages SET deleted_at = @DeletedAt WHERE id = @Id",
            new { Id = id, DeletedAt = DateTime.UtcNow });
    }
}

public class ConversationRepository : IConversationRepository
{
    private readonly IConnectionFactory _connectionFactory;

    public ConversationRepository(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Conversation?> GetByIdAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateReadConnection();
        return await connection.QueryFirstOrDefaultAsync<Conversation>(
            "SELECT * FROM conversations WHERE id = @Id AND is_active = true",
            new { Id = id });
    }

    public async Task<Conversation?> GetDirectConversationAsync(Guid userId1, Guid userId2)
    {
        using var connection = _connectionFactory.CreateReadConnection();
        return await connection.QueryFirstOrDefaultAsync<Conversation>(
            @"SELECT c.* FROM conversations c
              INNER JOIN conversation_participants p1 ON c.id = p1.conversation_id AND p1.user_id = @UserId1
              INNER JOIN conversation_participants p2 ON c.id = p2.conversation_id AND p2.user_id = @UserId2
              WHERE c.type = 0 AND c.is_active = true",
            new { UserId1 = userId1, UserId2 = userId2 });
    }

    public async Task<IEnumerable<ConversationWithParticipant>> GetUserConversationsAsync(Guid userId, int page, int pageSize, bool includeArchived = false)
    {
        using var connection = _connectionFactory.CreateReadConnection();
        var offset = (page - 1) * pageSize;
        
        return await connection.QueryAsync<ConversationWithParticipant>(
            @"SELECT c.*, p.unread_count, p.is_muted, p.is_archived, p.last_read_at
              FROM conversations c
              INNER JOIN conversation_participants p ON c.id = p.conversation_id
              WHERE p.user_id = @UserId 
              AND p.left_at IS NULL
              AND c.is_active = true
              AND (@IncludeArchived = true OR p.is_archived = false)
              ORDER BY c.last_message_at DESC NULLS LAST
              LIMIT @PageSize OFFSET @Offset",
            new { UserId = userId, IncludeArchived = includeArchived, PageSize = pageSize, Offset = offset });
    }

    public async Task<Guid> CreateAsync(Conversation conversation)
    {
        using var connection = _connectionFactory.CreateWriteConnection();
        var id = Guid.NewGuid();
        await connection.ExecuteAsync(
            @"INSERT INTO conversations (id, title, type, project_id, order_id, created_at, updated_at, is_active)
              VALUES (@Id, @Title, @Type, @ProjectId, @OrderId, @CreatedAt, @UpdatedAt, @IsActive)",
            new
            {
                Id = id,
                conversation.Title,
                conversation.Type,
                conversation.ProjectId,
                conversation.OrderId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            });
        return id;
    }

    public async Task UpdateLastMessageAsync(Guid conversationId, Guid messageId)
    {
        using var connection = _connectionFactory.CreateWriteConnection();
        await connection.ExecuteAsync(
            @"UPDATE conversations 
              SET last_message_id = @MessageId, last_message_at = @Now, updated_at = @Now
              WHERE id = @ConversationId",
            new { ConversationId = conversationId, MessageId = messageId, Now = DateTime.UtcNow });
    }

    public async Task<ConversationParticipant?> GetParticipantAsync(Guid conversationId, Guid userId)
    {
        using var connection = _connectionFactory.CreateReadConnection();
        return await connection.QueryFirstOrDefaultAsync<ConversationParticipant>(
            "SELECT * FROM conversation_participants WHERE conversation_id = @ConversationId AND user_id = @UserId",
            new { ConversationId = conversationId, UserId = userId });
    }

    public async Task AddParticipantAsync(ConversationParticipant participant)
    {
        using var connection = _connectionFactory.CreateWriteConnection();
        await connection.ExecuteAsync(
            @"INSERT INTO conversation_participants (id, conversation_id, user_id, role, joined_at)
              VALUES (@Id, @ConversationId, @UserId, @Role, @JoinedAt)",
            new
            {
                Id = Guid.NewGuid(),
                participant.ConversationId,
                participant.UserId,
                participant.Role,
                JoinedAt = DateTime.UtcNow
            });
    }

    public async Task UpdateParticipantAsync(ConversationParticipant participant)
    {
        using var connection = _connectionFactory.CreateWriteConnection();
        await connection.ExecuteAsync(
            @"UPDATE conversation_participants 
              SET is_muted = @IsMuted, is_archived = @IsArchived
              WHERE conversation_id = @ConversationId AND user_id = @UserId",
            new { participant.ConversationId, participant.UserId, participant.IsMuted, participant.IsArchived });
    }

    public async Task RemoveParticipantAsync(Guid conversationId, Guid userId)
    {
        using var connection = _connectionFactory.CreateWriteConnection();
        await connection.ExecuteAsync(
            @"UPDATE conversation_participants 
              SET left_at = @LeftAt
              WHERE conversation_id = @ConversationId AND user_id = @UserId",
            new { ConversationId = conversationId, UserId = userId, LeftAt = DateTime.UtcNow });
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        using var connection = _connectionFactory.CreateReadConnection();
        return await connection.ExecuteScalarAsync<int>(
            @"SELECT COALESCE(SUM(unread_count), 0) FROM conversation_participants
              WHERE user_id = @UserId AND left_at IS NULL AND is_archived = false",
            new { UserId = userId });
    }

    public async Task MarkAsReadAsync(Guid conversationId, Guid userId, Guid messageId)
    {
        using var connection = _connectionFactory.CreateWriteConnection();
        await connection.ExecuteAsync(
            @"UPDATE conversation_participants 
              SET last_read_message_id = @MessageId, last_read_at = @Now, unread_count = 0
              WHERE conversation_id = @ConversationId AND user_id = @UserId",
            new { ConversationId = conversationId, UserId = userId, MessageId = messageId, Now = DateTime.UtcNow });
    }
}

public record ConversationWithParticipant : Conversation
{
    public int UnreadCount { get; init; }
    public bool IsMuted { get; init; }
    public bool IsArchived { get; init; }
    public DateTime? LastReadAt { get; init; }
}
