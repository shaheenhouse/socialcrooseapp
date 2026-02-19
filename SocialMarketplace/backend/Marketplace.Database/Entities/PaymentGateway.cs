namespace Marketplace.Database.Entities;

public class PaymentGateway : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty; // stripe, paypal, jazzcash, easypaisa
    public string? DisplayName { get; set; }
    public string? LogoUrl { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; }
    public string? SupportedCurrencies { get; set; } // JSON array
    public string? SupportedCountries { get; set; } // JSON array
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public decimal FeePercentage { get; set; }
    public decimal FeeFixed { get; set; }
    public string? ApiKey { get; set; } // Encrypted
    public string? SecretKey { get; set; } // Encrypted
    public string? WebhookSecret { get; set; } // Encrypted
    public string? Configuration { get; set; } // JSON
    public bool IsTestMode { get; set; }
    public int SortOrder { get; set; }
    public string? PaymentMethods { get; set; } // JSON: ["card", "bank", "wallet"]
}
