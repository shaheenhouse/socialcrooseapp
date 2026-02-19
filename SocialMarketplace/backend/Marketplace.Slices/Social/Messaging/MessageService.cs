using Marketplace.Core.Infrastructure;
using Marketplace.Database.Entities.Social;

namespace Marketplace.Slices.Social.Messaging;

public interface IMessageService
{
    Task<ConversationDto?> GetConversationAsync(Guid conversationId, Guid userId);
    Task<IEnumerable<ConversationListDto>> GetConversationsAsync(Guid userId, int page, int pageSize, bool includeArchived);
    Task<IEnumerable<MessageDto>> GetMessagesAsync(Guid conversationId, Guid userId, int page, int pageSize, Guid? beforeMessageId);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task<ConversationDto> GetOrCreateDirectConversationAsync(Guid userId, Guid otherUserId);
    Task<ConversationDto> CreateGroupConversationAsync(Guid creatorId, string title, IEnumerable<Guid> participantIds);
    Task<MessageDto> SendMessageAsync(Guid conversationId, Guid senderId, SendMessageRequest request);
    Task<MessageDto> EditMessageAsync(Guid messageId, Guid userId, string newContent);
    Task DeleteMessageAsync(Guid messageId, Guid userId);
    Task MarkAsReadAsync(Guid conversationId, Guid userId);
    Task MuteConversationAsync(Guid conversationId, Guid userId, bool muted);
    Task ArchiveConversationAsync(Guid conversationId, Guid userId, bool archived);
    Task LeaveConversationAsync(Guid conversationId, Guid userId);
}

public class MessageService : IMessageService
{
    private readonly IMessageRepository _messageRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly IJobQueue _jobQueue;

    public MessageService(
        IMessageRepository messageRepository,
        IConversationRepository conversationRepository,
        IJobQueue jobQueue)
    {
        _messageRepository = messageRepository;
        _conversationRepository = conversationRepository;
        _jobQueue = jobQueue;
    }

    public async Task<ConversationDto?> GetConversationAsync(Guid conversationId, Guid userId)
    {
        var conversation = await _conversationRepository.GetByIdAsync(conversationId);
        if (conversation == null) return null;

        var participant = await _conversationRepository.GetParticipantAsync(conversationId, userId);
        if (participant == null || participant.LeftAt.HasValue) return null;

        return MapConversationToDto(conversation, participant);
    }

    public async Task<IEnumerable<ConversationListDto>> GetConversationsAsync(Guid userId, int page, int pageSize, bool includeArchived)
    {
        var conversations = await _conversationRepository.GetUserConversationsAsync(userId, page, pageSize, includeArchived);
        return conversations.Select(c => new ConversationListDto
        {
            Id = c.Id,
            Title = c.Title,
            Type = c.Type,
            LastMessageAt = c.LastMessageAt,
            UnreadCount = c.UnreadCount,
            IsMuted = c.IsMuted,
            IsArchived = c.IsArchived
        });
    }

    public async Task<IEnumerable<MessageDto>> GetMessagesAsync(Guid conversationId, Guid userId, int page, int pageSize, Guid? beforeMessageId)
    {
        // Verify user is participant
        var participant = await _conversationRepository.GetParticipantAsync(conversationId, userId);
        if (participant == null || participant.LeftAt.HasValue)
            throw new UnauthorizedAccessException("Not a participant in this conversation");

        var messages = await _messageRepository.GetConversationMessagesAsync(conversationId, page, pageSize, beforeMessageId);
        return messages.Select(MapMessageToDto);
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        return await _conversationRepository.GetUnreadCountAsync(userId);
    }

    public async Task<ConversationDto> GetOrCreateDirectConversationAsync(Guid userId, Guid otherUserId)
    {
        var existing = await _conversationRepository.GetDirectConversationAsync(userId, otherUserId);
        if (existing != null)
        {
            var participant = await _conversationRepository.GetParticipantAsync(existing.Id, userId);
            return MapConversationToDto(existing, participant);
        }

        // Create new conversation
        var conversation = new Conversation
        {
            Type = ConversationType.Direct
        };
        var conversationId = await _conversationRepository.CreateAsync(conversation);

        // Add participants
        await _conversationRepository.AddParticipantAsync(new ConversationParticipant
        {
            ConversationId = conversationId,
            UserId = userId,
            Role = ParticipantRole.Member
        });
        await _conversationRepository.AddParticipantAsync(new ConversationParticipant
        {
            ConversationId = conversationId,
            UserId = otherUserId,
            Role = ParticipantRole.Member
        });

        var newConversation = await _conversationRepository.GetByIdAsync(conversationId);
        var newParticipant = await _conversationRepository.GetParticipantAsync(conversationId, userId);
        return MapConversationToDto(newConversation!, newParticipant);
    }

    public async Task<ConversationDto> CreateGroupConversationAsync(Guid creatorId, string title, IEnumerable<Guid> participantIds)
    {
        var conversation = new Conversation
        {
            Title = title,
            Type = ConversationType.Group
        };
        var conversationId = await _conversationRepository.CreateAsync(conversation);

        // Add creator as owner
        await _conversationRepository.AddParticipantAsync(new ConversationParticipant
        {
            ConversationId = conversationId,
            UserId = creatorId,
            Role = ParticipantRole.Owner
        });

        // Add other participants
        foreach (var participantId in participantIds.Where(p => p != creatorId))
        {
            await _conversationRepository.AddParticipantAsync(new ConversationParticipant
            {
                ConversationId = conversationId,
                UserId = participantId,
                Role = ParticipantRole.Member
            });
        }

        var newConversation = await _conversationRepository.GetByIdAsync(conversationId);
        var creatorParticipant = await _conversationRepository.GetParticipantAsync(conversationId, creatorId);
        return MapConversationToDto(newConversation!, creatorParticipant);
    }

    public async Task<MessageDto> SendMessageAsync(Guid conversationId, Guid senderId, SendMessageRequest request)
    {
        // Verify sender is participant
        var participant = await _conversationRepository.GetParticipantAsync(conversationId, senderId);
        if (participant == null || participant.LeftAt.HasValue)
            throw new UnauthorizedAccessException("Not a participant in this conversation");

        var message = new Message
        {
            ConversationId = conversationId,
            SenderId = senderId,
            Content = request.Content,
            Type = request.Type,
            AttachmentUrl = request.AttachmentUrl,
            AttachmentName = request.AttachmentName,
            AttachmentSize = request.AttachmentSize,
            ReplyToId = request.ReplyToId
        };

        var messageId = await _messageRepository.CreateAsync(message);
        await _conversationRepository.UpdateLastMessageAsync(conversationId, messageId);

        // Queue notification for other participants
        await _jobQueue.EnqueueAsync("notifications", new Dictionary<string, string>
        {
            ["Type"] = "new_message",
            ["ConversationId"] = conversationId.ToString(),
            ["SenderId"] = senderId.ToString(),
            ["MessageId"] = messageId.ToString()
        });

        // Push real-time update
        await _jobQueue.EnqueueAsync("realtime-push", new Dictionary<string, string>
        {
            ["Event"] = "message:new",
            ["ConversationId"] = conversationId.ToString(),
            ["MessageId"] = messageId.ToString()
        });

        var createdMessage = await _messageRepository.GetByIdAsync(messageId);
        return MapMessageToDto(createdMessage!);
    }

    public async Task<MessageDto> EditMessageAsync(Guid messageId, Guid userId, string newContent)
    {
        var message = await _messageRepository.GetByIdAsync(messageId);
        if (message == null || message.SenderId != userId)
            throw new UnauthorizedAccessException("Cannot edit this message");

        message.Content = newContent;
        await _messageRepository.UpdateAsync(message);

        var updatedMessage = await _messageRepository.GetByIdAsync(messageId);
        return MapMessageToDto(updatedMessage!);
    }

    public async Task DeleteMessageAsync(Guid messageId, Guid userId)
    {
        var message = await _messageRepository.GetByIdAsync(messageId);
        if (message == null || message.SenderId != userId)
            throw new UnauthorizedAccessException("Cannot delete this message");

        await _messageRepository.SoftDeleteAsync(messageId);
    }

    public async Task MarkAsReadAsync(Guid conversationId, Guid userId)
    {
        var conversation = await _conversationRepository.GetByIdAsync(conversationId);
        if (conversation?.LastMessageId == null) return;

        await _conversationRepository.MarkAsReadAsync(conversationId, userId, conversation.LastMessageId.Value);
    }

    public async Task MuteConversationAsync(Guid conversationId, Guid userId, bool muted)
    {
        var participant = await _conversationRepository.GetParticipantAsync(conversationId, userId);
        if (participant == null)
            throw new UnauthorizedAccessException("Not a participant in this conversation");

        participant.IsMuted = muted;
        await _conversationRepository.UpdateParticipantAsync(participant);
    }

    public async Task ArchiveConversationAsync(Guid conversationId, Guid userId, bool archived)
    {
        var participant = await _conversationRepository.GetParticipantAsync(conversationId, userId);
        if (participant == null)
            throw new UnauthorizedAccessException("Not a participant in this conversation");

        participant.IsArchived = archived;
        await _conversationRepository.UpdateParticipantAsync(participant);
    }

    public async Task LeaveConversationAsync(Guid conversationId, Guid userId)
    {
        await _conversationRepository.RemoveParticipantAsync(conversationId, userId);
    }

    private static ConversationDto MapConversationToDto(Conversation conversation, ConversationParticipant? participant)
    {
        return new ConversationDto
        {
            Id = conversation.Id,
            Title = conversation.Title,
            Type = conversation.Type,
            ProjectId = conversation.ProjectId,
            OrderId = conversation.OrderId,
            LastMessageAt = conversation.LastMessageAt,
            CreatedAt = conversation.CreatedAt,
            UnreadCount = participant?.UnreadCount ?? 0,
            IsMuted = participant?.IsMuted ?? false,
            IsArchived = participant?.IsArchived ?? false,
            MyRole = participant?.Role ?? ParticipantRole.Member
        };
    }

    private static MessageDto MapMessageToDto(Message message)
    {
        return new MessageDto
        {
            Id = message.Id,
            ConversationId = message.ConversationId,
            SenderId = message.SenderId,
            Content = message.Content,
            Type = message.Type,
            AttachmentUrl = message.AttachmentUrl,
            AttachmentName = message.AttachmentName,
            AttachmentSize = message.AttachmentSize,
            ReplyToId = message.ReplyToId,
            IsEdited = message.IsEdited,
            CreatedAt = message.CreatedAt,
            EditedAt = message.EditedAt
        };
    }
}

public record ConversationDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public ConversationType Type { get; init; }
    public Guid? ProjectId { get; init; }
    public Guid? OrderId { get; init; }
    public DateTime? LastMessageAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public int UnreadCount { get; init; }
    public bool IsMuted { get; init; }
    public bool IsArchived { get; init; }
    public ParticipantRole MyRole { get; init; }
}

public record ConversationListDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public ConversationType Type { get; init; }
    public DateTime? LastMessageAt { get; init; }
    public int UnreadCount { get; init; }
    public bool IsMuted { get; init; }
    public bool IsArchived { get; init; }
}

public record MessageDto
{
    public Guid Id { get; init; }
    public Guid ConversationId { get; init; }
    public Guid SenderId { get; init; }
    public string Content { get; init; } = string.Empty;
    public MessageType Type { get; init; }
    public string? AttachmentUrl { get; init; }
    public string? AttachmentName { get; init; }
    public long? AttachmentSize { get; init; }
    public Guid? ReplyToId { get; init; }
    public bool IsEdited { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? EditedAt { get; init; }
}

public record SendMessageRequest
{
    public string Content { get; init; } = string.Empty;
    public MessageType Type { get; init; } = MessageType.Text;
    public string? AttachmentUrl { get; init; }
    public string? AttachmentName { get; init; }
    public long? AttachmentSize { get; init; }
    public Guid? ReplyToId { get; init; }
}
