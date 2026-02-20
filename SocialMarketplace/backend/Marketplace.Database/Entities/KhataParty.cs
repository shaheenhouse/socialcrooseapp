namespace Marketplace.Database.Entities;

public class KhataParty : BaseEntity
{
    public Guid UserId { get; set; }
    public string PartyName { get; set; } = string.Empty;
    public string? PartyPhone { get; set; }
    public string? PartyAddress { get; set; }
    public string Type { get; set; } = "customer"; // customer | supplier
    public decimal OpeningBalance { get; set; }
    public decimal TotalCredit { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal Balance { get; set; }
    public DateTime? LastTransactionAt { get; set; }
    public string? Notes { get; set; }

    public User? User { get; set; }
    public ICollection<KhataEntry> Entries { get; set; } = new List<KhataEntry>();
}
