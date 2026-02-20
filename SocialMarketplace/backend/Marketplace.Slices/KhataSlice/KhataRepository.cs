using System.Data;
using Dapper;
using Marketplace.Core.Infrastructure;

namespace Marketplace.Slices.KhataSlice;

public interface IKhataRepository
{
    Task<PagedResult<KhataPartyDto>> GetPartiesAsync(Guid userId, KhataQueryParams query);
    Task<KhataPartyDto?> GetPartyByIdAsync(Guid userId, Guid partyId);
    Task<KhataPartyDto> CreatePartyAsync(Guid userId, CreateKhataPartyDto dto);
    Task<bool> UpdatePartyAsync(Guid userId, Guid partyId, UpdateKhataPartyDto dto);
    Task<bool> DeletePartyAsync(Guid userId, Guid partyId);
    Task<PagedResult<KhataEntryDto>> GetEntriesAsync(Guid userId, Guid partyId, KhataEntryQueryParams query);
    Task<KhataEntryDto> AddEntryAsync(Guid userId, Guid partyId, CreateKhataEntryDto dto);
    Task<KhataSummaryDto> GetSummaryAsync(Guid userId, DateRangeParams? range);
}

public class KhataRepository : IKhataRepository
{
    private readonly IConnectionFactory _connectionFactory;

    public KhataRepository(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<PagedResult<KhataPartyDto>> GetPartiesAsync(Guid userId, KhataQueryParams query)
    {
        using var connection = _connectionFactory.CreateReadConnection();

        var whereClause = "WHERE kp.user_id = @UserId AND kp.is_deleted = false";
        if (!string.IsNullOrEmpty(query.Search))
            whereClause += " AND (kp.party_name ILIKE @Search OR kp.party_phone ILIKE @Search)";
        if (!string.IsNullOrEmpty(query.Status))
            whereClause += " AND kp.type = @Status";

        var countSql = $"SELECT COUNT(*) FROM khata_parties kp {whereClause}";
        var total = await connection.ExecuteScalarAsync<int>(countSql, new
        {
            UserId = userId,
            Search = $"%{query.Search}%",
            Status = query.Status
        });

        var sql = $@"
            SELECT id, party_name, party_phone, party_address, type,
                   total_credit, total_debit, balance, last_transaction_at, created_at
            FROM khata_parties kp
            {whereClause}
            ORDER BY last_transaction_at DESC NULLS LAST, created_at DESC
            LIMIT @PageSize OFFSET @Offset";

        var items = await connection.QueryAsync<KhataPartyDto>(sql, new
        {
            UserId = userId,
            Search = $"%{query.Search}%",
            Status = query.Status,
            PageSize = query.PageSize,
            Offset = (query.Page - 1) * query.PageSize
        });

        return new PagedResult<KhataPartyDto>(items, total, query.Page, query.PageSize);
    }

    public async Task<KhataPartyDto?> GetPartyByIdAsync(Guid userId, Guid partyId)
    {
        using var connection = _connectionFactory.CreateReadConnection();
        return await connection.QuerySingleOrDefaultAsync<KhataPartyDto>(
            @"SELECT id, party_name, party_phone, party_address, type,
                     total_credit, total_debit, balance, last_transaction_at, created_at
              FROM khata_parties WHERE id = @PartyId AND user_id = @UserId AND is_deleted = false",
            new { PartyId = partyId, UserId = userId });
    }

    public async Task<KhataPartyDto> CreatePartyAsync(Guid userId, CreateKhataPartyDto dto)
    {
        using var connection = _connectionFactory.CreateWriteConnection();
        var id = Guid.NewGuid();
        var balance = dto.OpeningBalance ?? 0;

        await connection.ExecuteAsync(
            @"INSERT INTO khata_parties (id, user_id, party_name, party_phone, party_address, type, opening_balance, balance, total_credit, total_debit, created_at, is_deleted)
              VALUES (@Id, @UserId, @PartyName, @PartyPhone, @PartyAddress, @Type, @OpeningBalance, @Balance, 0, 0, @CreatedAt, false)",
            new
            {
                Id = id, UserId = userId, dto.PartyName, dto.PartyPhone, dto.PartyAddress,
                dto.Type, OpeningBalance = balance, Balance = balance, CreatedAt = DateTime.UtcNow
            });

        return (await GetPartyByIdAsync(userId, id))!;
    }

    public async Task<bool> UpdatePartyAsync(Guid userId, Guid partyId, UpdateKhataPartyDto dto)
    {
        using var connection = _connectionFactory.CreateWriteConnection();
        var rows = await connection.ExecuteAsync(
            @"UPDATE khata_parties SET
                party_name = COALESCE(@PartyName, party_name),
                party_phone = COALESCE(@PartyPhone, party_phone),
                party_address = COALESCE(@PartyAddress, party_address),
                notes = COALESCE(@Notes, notes),
                updated_at = @UpdatedAt
              WHERE id = @PartyId AND user_id = @UserId AND is_deleted = false",
            new { dto.PartyName, dto.PartyPhone, dto.PartyAddress, dto.Notes, PartyId = partyId, UserId = userId, UpdatedAt = DateTime.UtcNow });
        return rows > 0;
    }

    public async Task<bool> DeletePartyAsync(Guid userId, Guid partyId)
    {
        using var connection = _connectionFactory.CreateWriteConnection();
        var rows = await connection.ExecuteAsync(
            "UPDATE khata_parties SET is_deleted = true, deleted_at = @Now WHERE id = @PartyId AND user_id = @UserId",
            new { PartyId = partyId, UserId = userId, Now = DateTime.UtcNow });
        return rows > 0;
    }

    public async Task<PagedResult<KhataEntryDto>> GetEntriesAsync(Guid userId, Guid partyId, KhataEntryQueryParams query)
    {
        using var connection = _connectionFactory.CreateReadConnection();

        var whereClause = "WHERE ke.khata_party_id = @PartyId AND ke.is_deleted = false AND kp.user_id = @UserId";
        if (!string.IsNullOrEmpty(query.Type))
            whereClause += " AND ke.type = @Type";

        var countSql = $"SELECT COUNT(*) FROM khata_entries ke JOIN khata_parties kp ON kp.id = ke.khata_party_id {whereClause}";
        var total = await connection.ExecuteScalarAsync<int>(countSql, new { PartyId = partyId, UserId = userId, query.Type });

        var sql = $@"
            SELECT ke.id, ke.amount, ke.type, ke.description, ke.transaction_date, ke.running_balance, ke.attachment_url, ke.created_at
            FROM khata_entries ke JOIN khata_parties kp ON kp.id = ke.khata_party_id
            {whereClause}
            ORDER BY ke.transaction_date DESC, ke.created_at DESC
            LIMIT @PageSize OFFSET @Offset";

        var items = await connection.QueryAsync<KhataEntryDto>(sql, new
        {
            PartyId = partyId, UserId = userId, query.Type,
            PageSize = query.PageSize, Offset = (query.Page - 1) * query.PageSize
        });

        return new PagedResult<KhataEntryDto>(items, total, query.Page, query.PageSize);
    }

    public async Task<KhataEntryDto> AddEntryAsync(Guid userId, Guid partyId, CreateKhataEntryDto dto)
    {
        using var connection = _connectionFactory.CreateWriteConnection();
        if (connection.State != ConnectionState.Open)
            await ((System.Data.Common.DbConnection)connection).OpenAsync();

        using var tx = ((System.Data.Common.DbConnection)connection).BeginTransaction();

        var party = await connection.QuerySingleOrDefaultAsync<dynamic>(
            "SELECT balance FROM khata_parties WHERE id = @PartyId AND user_id = @UserId AND is_deleted = false FOR UPDATE",
            new { PartyId = partyId, UserId = userId }, tx);

        if (party == null) throw new InvalidOperationException("Party not found");

        decimal currentBalance = party.balance;
        decimal newBalance = dto.Type == "credit" ? currentBalance + dto.Amount : currentBalance - dto.Amount;

        var entryId = Guid.NewGuid();
        var txDate = string.IsNullOrEmpty(dto.Date) ? DateTime.UtcNow : DateTime.Parse(dto.Date);

        await connection.ExecuteAsync(
            @"INSERT INTO khata_entries (id, khata_party_id, amount, type, description, transaction_date, running_balance, attachment_url, created_at, is_deleted)
              VALUES (@Id, @PartyId, @Amount, @Type, @Description, @TxDate, @RunningBalance, @AttachmentUrl, @CreatedAt, false)",
            new
            {
                Id = entryId, PartyId = partyId, dto.Amount, dto.Type, dto.Description,
                TxDate = txDate, RunningBalance = newBalance, dto.AttachmentUrl, CreatedAt = DateTime.UtcNow
            }, tx);

        var creditInc = dto.Type == "credit" ? dto.Amount : 0;
        var debitInc = dto.Type == "debit" ? dto.Amount : 0;

        await connection.ExecuteAsync(
            @"UPDATE khata_parties SET
                balance = @NewBalance, total_credit = total_credit + @CreditInc, total_debit = total_debit + @DebitInc,
                last_transaction_at = @Now, updated_at = @Now
              WHERE id = @PartyId",
            new { NewBalance = newBalance, CreditInc = creditInc, DebitInc = debitInc, PartyId = partyId, Now = DateTime.UtcNow }, tx);

        tx.Commit();

        return new KhataEntryDto(entryId, dto.Amount, dto.Type, dto.Description, txDate, newBalance, dto.AttachmentUrl, DateTime.UtcNow);
    }

    public async Task<KhataSummaryDto> GetSummaryAsync(Guid userId, DateRangeParams? range)
    {
        using var connection = _connectionFactory.CreateReadConnection();
        var result = await connection.QuerySingleAsync<KhataSummaryDto>(
            @"SELECT
                COALESCE(SUM(CASE WHEN balance > 0 THEN balance ELSE 0 END), 0) as total_receivable,
                COALESCE(SUM(CASE WHEN balance < 0 THEN ABS(balance) ELSE 0 END), 0) as total_payable,
                COALESCE(SUM(balance), 0) as net_balance,
                COUNT(*) as total_parties,
                (SELECT COUNT(*) FROM khata_entries ke JOIN khata_parties kp ON kp.id = ke.khata_party_id WHERE kp.user_id = @UserId AND ke.is_deleted = false) as total_transactions
              FROM khata_parties WHERE user_id = @UserId AND is_deleted = false",
            new { UserId = userId });
        return result;
    }
}
