namespace Marketplace.Database.Enums;

public enum EscrowStatus
{
    Pending = 0,
    Funded = 1,
    Released = 2,
    PartiallyReleased = 3,
    Disputed = 4,
    Refunded = 5,
    Cancelled = 6
}
