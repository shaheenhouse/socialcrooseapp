namespace Marketplace.Database.Entities;

public class Invoice : BaseEntity
{
    public Guid UserId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string? ClientEmail { get; set; }
    public string? ClientPhone { get; set; }
    public string? ClientAddress { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public string Status { get; set; } = "draft"; // draft | sent | viewed | paid | overdue | cancelled
    public decimal Subtotal { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal Total { get; set; }
    public string Currency { get; set; } = "PKR";
    public string? Notes { get; set; }
    public string? Terms { get; set; }
    public string? PublicToken { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? PaymentMethod { get; set; }

    public User? User { get; set; }
    public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
}

public class InvoiceItem : BaseEntity
{
    public Guid InvoiceId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = "unit";
    public decimal UnitPrice { get; set; }
    public decimal Total { get; set; }

    public Invoice? Invoice { get; set; }
}
