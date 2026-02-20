using Dapper;
using Microsoft.Extensions.Logging;
using Marketplace.Core.Infrastructure;

namespace Marketplace.Slices.WalletSlice;

public interface IWalletRepository
{
    Task<WalletDto?> GetByUserIdAsync(Guid userId);
    Task<Guid> CreateAsync(Guid userId, string currency);
    Task<(IEnumerable<TransactionDto> Transactions, int TotalCount)> GetTransactionsAsync(Guid walletId, int page, int pageSize, string? type = null);
    Task<IEnumerable<EscrowDto>> GetUserEscrowsAsync(Guid userId, int page, int pageSize);
}

public interface IWalletService
{
    Task<WalletDto?> GetMyWalletAsync(Guid userId);
    Task<(IEnumerable<TransactionDto> Transactions, int TotalCount)> GetTransactionsAsync(Guid userId, int page, int pageSize, string? type = null);
    Task<IEnumerable<EscrowDto>> GetEscrowsAsync(Guid userId, int page, int pageSize);
}

public class WalletRepository : IWalletRepository
{
    private readonly IConnectionFactory _connectionFactory;

    public WalletRepository(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<WalletDto?> GetByUserIdAsync(Guid userId)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<WalletDto>("""
            SELECT id, "UserId", balance, "PendingBalance",
                   "HeldBalance", currency, "IsActive",
                   "TotalEarned", "TotalWithdrawn",
                   "TotalSpent", "CreatedAt"
            FROM wallets WHERE "UserId" = @UserId AND "IsDeleted" = false
            """, new { UserId = userId });
    }

    public async Task<Guid> CreateAsync(Guid userId, string currency)
    {
        using var connection = await _connectionFactory.CreateWriteConnectionAsync();
        var id = Guid.NewGuid();
        await connection.ExecuteAsync("""
            INSERT INTO wallets (id, "UserId", balance, "PendingBalance", "HeldBalance", currency,
                                "IsActive", "IsLocked", "TotalEarned", "TotalWithdrawn", "TotalSpent",
                                "CreatedAt", "IsDeleted")
            VALUES (@Id, @UserId, 0, 0, 0, @Currency,
                   true, false, 0, 0, 0,
                   NOW(), false)
            """, new { Id = id, UserId = userId, Currency = currency });
        return id;
    }

    public async Task<(IEnumerable<TransactionDto> Transactions, int TotalCount)> GetTransactionsAsync(Guid walletId, int page, int pageSize, string? type = null)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();

        var whereClause = @"WHERE t.wallet_id = @WalletId AND t.""IsDeleted"" = false";
        if (!string.IsNullOrEmpty(type))
            whereClause += " AND t.type = @Type::integer";

        var totalCount = await connection.ExecuteScalarAsync<int>(
            $"SELECT COUNT(*) FROM transactions t {whereClause}", new { WalletId = walletId, Type = type });

        var transactions = await connection.QueryAsync<TransactionDto>($"""
            SELECT t.id, t."TransactionNumber", t.type, t.amount,
                   t."BalanceBefore", t."BalanceAfter",
                   t.currency, t.description, t.reference, t.status, t."CreatedAt"
            FROM transactions t
            {whereClause}
            ORDER BY t."CreatedAt" DESC
            LIMIT @PageSize OFFSET @Offset
            """, new { WalletId = walletId, Type = type, PageSize = pageSize, Offset = (page - 1) * pageSize });

        return (transactions, totalCount);
    }

    public async Task<IEnumerable<EscrowDto>> GetUserEscrowsAsync(Guid userId, int page, int pageSize)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();
        return await connection.QueryAsync<EscrowDto>("""
            SELECT e.id, e."EscrowNumber", e.amount, e."ReleasedAmount",
                   e."RefundedAmount", e."HeldAmount",
                   e.currency, e.status, e."CreatedAt",
                   CASE WHEN e."BuyerId" = @UserId THEN 'Buyer' ELSE 'Seller' END as UserRole,
                   b."FirstName" || ' ' || b."LastName" as BuyerName,
                   s."FirstName" || ' ' || s."LastName" as SellerName
            FROM escrows e
            JOIN users b ON e."BuyerId" = b.id
            JOIN users s ON e."SellerId" = s.id
            WHERE (e."BuyerId" = @UserId OR e."SellerId" = @UserId) AND e."IsDeleted" = false
            ORDER BY e."CreatedAt" DESC
            LIMIT @PageSize OFFSET @Offset
            """, new { UserId = userId, PageSize = pageSize, Offset = (page - 1) * pageSize });
    }
}

public class WalletService : IWalletService
{
    private readonly IWalletRepository _repository;
    private readonly ILogger<WalletService> _logger;

    public WalletService(IWalletRepository repository, ILogger<WalletService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<WalletDto?> GetMyWalletAsync(Guid userId)
    {
        var wallet = await _repository.GetByUserIdAsync(userId);
        if (wallet == null)
        {
            await _repository.CreateAsync(userId, "USD");
            wallet = await _repository.GetByUserIdAsync(userId);
        }
        return wallet;
    }

    public async Task<(IEnumerable<TransactionDto> Transactions, int TotalCount)> GetTransactionsAsync(Guid userId, int page, int pageSize, string? type = null)
    {
        var wallet = await GetMyWalletAsync(userId);
        if (wallet == null) return ([], 0);
        return await _repository.GetTransactionsAsync(wallet.Id, page, pageSize, type);
    }

    public async Task<IEnumerable<EscrowDto>> GetEscrowsAsync(Guid userId, int page, int pageSize)
        => await _repository.GetUserEscrowsAsync(userId, page, pageSize);
}

// DTOs
public record WalletDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public decimal Balance { get; init; }
    public decimal PendingBalance { get; init; }
    public decimal HeldBalance { get; init; }
    public string Currency { get; init; } = "USD";
    public bool IsActive { get; init; }
    public decimal TotalEarned { get; init; }
    public decimal TotalWithdrawn { get; init; }
    public decimal TotalSpent { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record TransactionDto
{
    public Guid Id { get; init; }
    public string TransactionNumber { get; init; } = string.Empty;
    public int Type { get; init; }
    public decimal Amount { get; init; }
    public decimal BalanceBefore { get; init; }
    public decimal BalanceAfter { get; init; }
    public string Currency { get; init; } = "USD";
    public string? Description { get; init; }
    public string? Reference { get; init; }
    public int Status { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record EscrowDto
{
    public Guid Id { get; init; }
    public string EscrowNumber { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public decimal ReleasedAmount { get; init; }
    public decimal RefundedAmount { get; init; }
    public decimal HeldAmount { get; init; }
    public string Currency { get; init; } = "USD";
    public int Status { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? UserRole { get; init; }
    public string? BuyerName { get; init; }
    public string? SellerName { get; init; }
}
