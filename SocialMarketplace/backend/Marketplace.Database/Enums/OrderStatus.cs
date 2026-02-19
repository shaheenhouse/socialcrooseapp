namespace Marketplace.Database.Enums;

public enum OrderStatus
{
    Pending = 0,
    Confirmed = 1,
    Processing = 2,
    Shipped = 3,
    Delivered = 4,
    Completed = 5,
    Cancelled = 6,
    Refunded = 7,
    Disputed = 8
}
