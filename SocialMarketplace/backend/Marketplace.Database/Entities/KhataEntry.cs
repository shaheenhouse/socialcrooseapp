namespace Marketplace.Database.Entities;

public class KhataEntry : BaseEntity
{
    public Guid KhataPartyId { get; set; }
    public decimal Amount { get; set; }
    public string Type { get; set; } = "credit"; // credit | debit
    public string Description { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public decimal RunningBalance { get; set; }
    public string? AttachmentUrl { get; set; }
    public string? ReferenceNumber { get; set; }

    public KhataParty? KhataParty { get; set; }
}
