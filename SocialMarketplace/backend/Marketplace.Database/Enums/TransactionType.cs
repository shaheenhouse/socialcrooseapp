namespace Marketplace.Database.Enums;

public enum TransactionType
{
    Deposit = 0,
    Withdrawal = 1,
    Payment = 2,
    Refund = 3,
    Commission = 4,
    Transfer = 5,
    Escrow = 6,
    EscrowRelease = 7,
    Payout = 8,
    Fee = 9
}
