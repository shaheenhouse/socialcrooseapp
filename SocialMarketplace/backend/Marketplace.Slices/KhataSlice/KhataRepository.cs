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

        var whereClause = @"WHERE kp.""UserId"" = @UserId AND kp.""IsDeleted"" = false";
        if (!string.IsNullOrEmpty(query.Search))
            whereClause += @" AND (kp.""PartyName"" ILIKE @Search OR kp.""PartyPhone"" ILIKE @Search)";
        if (!string.IsNullOrEmpty(query.Status))
            whereClause += @" AND kp.""Type"" = @Status";

        var countSql = $"SELECT COUNT(*) FROM khata_parties kp {whereClause}";
        var total = await connection.ExecuteScalarAsync<int>(countSql, new
        {
            UserId = userId,
            Search = $"%{query.Search}%",
            Status = query.Status
        });

        var sql = $@"
            SELECT ""Id"", ""PartyName"", ""PartyPhone"", ""PartyAddress"", ""Type"",
                   ""TotalCredit"", ""TotalDebit"", ""Balance"", ""LastTransactionAt"", ""CreatedAt""
            FROM khata_parties kp
            {whereClause}
            ORDER BY ""LastTransactionAt"" DESC NULLS LAST, ""CreatedAt"" DESC
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
            @"SELECT ""Id"", ""PartyName"", ""PartyPhone"", ""PartyAddress"", ""Type"",
                     ""TotalCredit"", ""TotalDebit"", ""Balance"", ""LastTransactionAt"", ""CreatedAt""
              FROM khata_parties WHERE ""Id"" = @PartyId AND ""UserId"" = @UserId AND ""IsDeleted"" = false",
            new { PartyId = partyId, UserId = userId });
    }

    public async Task<KhataPartyDto> CreatePartyAsync(Guid userId, CreateKhataPartyDto dto)
    {
        using var connection = _connectionFactory.CreateWriteConnection();
        var id = Guid.NewGuid();
        var balance = dto.OpeningBalance ?? 0;

        await connection.ExecuteAsync(
            @"INSERT INTO khata_parties (""Id"", ""UserId"", ""PartyName"", ""PartyPhone"", ""PartyAddress"", ""Type"", ""OpeningBalance"", ""Balance"", ""TotalCredit"", ""TotalDebit"", ""CreatedAt"", ""IsDeleted"")
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
                ""PartyName"" = COALESCE(@PartyName, ""PartyName""),
                ""PartyPhone"" = COALESCE(@PartyPhone, ""PartyPhone""),
                ""PartyAddress"" = COALESCE(@PartyAddress, ""PartyAddress""),
                ""Notes"" = COALESCE(@Notes, ""Notes""),
                ""UpdatedAt"" = @UpdatedAt
              WHERE ""Id"" = @PartyId AND ""UserId"" = @UserId AND ""IsDeleted"" = false",
            new { dto.PartyName, dto.PartyPhone, dto.PartyAddress, dto.Notes, PartyId = partyId, UserId = userId, UpdatedAt = DateTime.UtcNow });
        return rows > 0;
    }

    public async Task<bool> DeletePartyAsync(Guid userId, Guid partyId)
    {
        using var connection = _connectionFactory.CreateWriteConnection();
        var rows = await connection.ExecuteAsync(
            @"UPDATE khata_parties SET ""IsDeleted"" = true, ""DeletedAt"" = @Now WHERE ""Id"" = @PartyId AND ""UserId"" = @UserId",
            new { PartyId = partyId, UserId = userId, Now = DateTime.UtcNow });
        return rows > 0;
    }

    public async Task<PagedResult<KhataEntryDto>> GetEntriesAsync(Guid userId, Guid partyId, KhataEntryQueryParams query)
    {
        using var connection = _connectionFactory.CreateReadConnection();

        var whereClause = @"WHERE ke.""KhataPartyId"" = @PartyId AND ke.""IsDeleted"" = false AND kp.""UserId"" = @UserId";
        if (!string.IsNullOrEmpty(query.Type))
            whereClause += @" AND ke.""Type"" = @Type";

        var countSql = $@"SELECT COUNT(*) FROM khata_entries ke JOIN khata_parties kp ON kp.""Id"" = ke.""KhataPartyId"" {whereClause}";
        var total = await connection.ExecuteScalarAsync<int>(countSql, new { PartyId = partyId, UserId = userId, query.Type });

        var sql = $@"
            SELECT ke.""Id"", ke.""Amount"", ke.""Type"", ke.""Description"", ke.""TransactionDate"", ke.""RunningBalance"", ke.""AttachmentUrl"", ke.""CreatedAt""
            FROM khata_entries ke JOIN khata_parties kp ON kp.""Id"" = ke.""KhataPartyId""
            {whereClause}
            ORDER BY ke.""TransactionDate"" DESC, ke.""CreatedAt"" DESC
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
            @"SELECT ""Balance"" FROM khata_parties WHERE ""Id"" = @PartyId AND ""UserId"" = @UserId AND ""IsDeleted"" = false FOR UPDATE",
            new { PartyId = partyId, UserId = userId }, tx);

        if (party == null) throw new InvalidOperationException("Party not found");

        decimal currentBalance = party.Balance;
        decimal newBalance = dto.Type == "credit" ? currentBalance + dto.Amount : currentBalance - dto.Amount;

        var entryId = Guid.NewGuid();
        var txDate = string.IsNullOrEmpty(dto.Date) ? DateTime.UtcNow : DateTime.Parse(dto.Date);

        await connection.ExecuteAsync(
            @"INSERT INTO khata_entries (""Id"", ""KhataPartyId"", ""Amount"", ""Type"", ""Description"", ""TransactionDate"", ""RunningBalance"", ""AttachmentUrl"", ""CreatedAt"", ""IsDeleted"")
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
                ""Balance"" = @NewBalance, ""TotalCredit"" = ""TotalCredit"" + @CreditInc, ""TotalDebit"" = ""TotalDebit"" + @DebitInc,
                ""LastTransactionAt"" = @Now, ""UpdatedAt"" = @Now
              WHERE ""Id"" = @PartyId",
            new { NewBalance = newBalance, CreditInc = creditInc, DebitInc = debitInc, PartyId = partyId, Now = DateTime.UtcNow }, tx);

        tx.Commit();

        return new KhataEntryDto(entryId, dto.Amount, dto.Type, dto.Description, txDate, newBalance, dto.AttachmentUrl, DateTime.UtcNow);
    }

    public async Task<KhataSummaryDto> GetSummaryAsync(Guid userId, DateRangeParams? range)
    {
        using var connection = _connectionFactory.CreateReadConnection();
        var result = await connection.QuerySingleAsync<KhataSummaryDto>(
            @"SELECT
                COALESCE(SUM(CASE WHEN ""Balance"" > 0 THEN ""Balance"" ELSE 0 END), 0) AS ""TotalReceivable"",
                COALESCE(SUM(CASE WHEN ""Balance"" < 0 THEN ABS(""Balance"") ELSE 0 END), 0) AS ""TotalPayable"",
                COALESCE(SUM(""Balance""), 0) AS ""NetBalance"",
                COUNT(*) AS ""TotalParties"",
                (SELECT COUNT(*) FROM khata_entries ke JOIN khata_parties kp ON kp.""Id"" = ke.""KhataPartyId"" WHERE kp.""UserId"" = @UserId AND ke.""IsDeleted"" = false) AS ""TotalTransactions""
              FROM khata_parties WHERE ""UserId"" = @UserId AND ""IsDeleted"" = false",
            new { UserId = userId });
        return result;
    }
}
