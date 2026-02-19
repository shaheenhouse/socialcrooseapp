namespace Marketplace.Database.Entities.Social;

/// <summary>
/// LinkedIn-style connection between users
/// </summary>
public class Connection
{
    public Guid Id { get; set; }
    public Guid RequesterId { get; set; }
    public Guid AddresseeId { get; set; }
    public ConnectionStatus Status { get; set; }
    public string? Message { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime? RejectedAt { get; set; }
    public DateTime? BlockedAt { get; set; }
}

public enum ConnectionStatus
{
    Pending = 0,
    Accepted = 1,
    Rejected = 2,
    Blocked = 3,
    Withdrawn = 4
}
