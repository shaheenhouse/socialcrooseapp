using Marketplace.Core.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Marketplace.Slices.KhataSlice;

public interface IKhataService
{
    Task<PagedResult<KhataPartyDto>> GetPartiesAsync(Guid userId, KhataQueryParams query);
    Task<KhataPartyDto?> GetPartyByIdAsync(Guid userId, Guid partyId);
    Task<KhataPartyDto> CreatePartyAsync(Guid userId, CreateKhataPartyDto dto);
    Task<bool> UpdatePartyAsync(Guid userId, Guid partyId, UpdateKhataPartyDto dto);
    Task<bool> DeletePartyAsync(Guid userId, Guid partyId);
    Task<PagedResult<KhataEntryDto>> GetEntriesAsync(Guid userId, Guid partyId, KhataEntryQueryParams query);
    Task<KhataEntryDto> AddEntryAsync(Guid userId, Guid partyId, CreateKhataEntryDto dto);
    Task<KhataSummaryDto> GetSummaryAsync(Guid userId, DateRangeParams? range);
    Task<bool> SendReminderAsync(Guid userId, Guid partyId);
}

public record KhataQueryParams(string? Search = null, string? Status = null, int Page = 1, int PageSize = 20);
public record KhataEntryQueryParams(string? Type = null, string? From = null, string? To = null, int Page = 1, int PageSize = 50);
public record DateRangeParams(string? From = null, string? To = null);
public record CreateKhataPartyDto(string PartyName, string? PartyPhone, string? PartyAddress, string Type, decimal? OpeningBalance);
public record UpdateKhataPartyDto(string? PartyName, string? PartyPhone, string? PartyAddress, string? Notes);
public record CreateKhataEntryDto(decimal Amount, string Type, string Description, string? Date, string? AttachmentUrl);

public record KhataPartyDto(Guid Id, string PartyName, string? PartyPhone, string? PartyAddress, string Type,
    decimal TotalCredit, decimal TotalDebit, decimal Balance, DateTime? LastTransactionAt, DateTime CreatedAt);

public record KhataEntryDto(Guid Id, decimal Amount, string Type, string Description, DateTime TransactionDate,
    decimal RunningBalance, string? AttachmentUrl, DateTime CreatedAt);

public record KhataSummaryDto(decimal TotalReceivable, decimal TotalPayable, decimal NetBalance, int TotalParties, int TotalTransactions);

public record PagedResult<T>(IEnumerable<T> Items, int Total, int Page, int PageSize);

public class KhataService : IKhataService
{
    private readonly IKhataRepository _repository;
    private readonly ILogger<KhataService> _logger;

    public KhataService(IKhataRepository repository, ILogger<KhataService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<PagedResult<KhataPartyDto>> GetPartiesAsync(Guid userId, KhataQueryParams query)
    {
        return await _repository.GetPartiesAsync(userId, query);
    }

    public async Task<KhataPartyDto?> GetPartyByIdAsync(Guid userId, Guid partyId)
    {
        return await _repository.GetPartyByIdAsync(userId, partyId);
    }

    public async Task<KhataPartyDto> CreatePartyAsync(Guid userId, CreateKhataPartyDto dto)
    {
        var result = await _repository.CreatePartyAsync(userId, dto);
        _logger.LogInformation("Khata party created: {PartyName} by user {UserId}", dto.PartyName, userId);
        return result;
    }

    public async Task<bool> UpdatePartyAsync(Guid userId, Guid partyId, UpdateKhataPartyDto dto)
    {
        return await _repository.UpdatePartyAsync(userId, partyId, dto);
    }

    public async Task<bool> DeletePartyAsync(Guid userId, Guid partyId)
    {
        return await _repository.DeletePartyAsync(userId, partyId);
    }

    public async Task<PagedResult<KhataEntryDto>> GetEntriesAsync(Guid userId, Guid partyId, KhataEntryQueryParams query)
    {
        return await _repository.GetEntriesAsync(userId, partyId, query);
    }

    public async Task<KhataEntryDto> AddEntryAsync(Guid userId, Guid partyId, CreateKhataEntryDto dto)
    {
        var result = await _repository.AddEntryAsync(userId, partyId, dto);
        _logger.LogInformation("Khata entry added: {Type} {Amount} for party {PartyId}", dto.Type, dto.Amount, partyId);
        return result;
    }

    public async Task<KhataSummaryDto> GetSummaryAsync(Guid userId, DateRangeParams? range)
    {
        return await _repository.GetSummaryAsync(userId, range);
    }

    public async Task<bool> SendReminderAsync(Guid userId, Guid partyId)
    {
        var party = await _repository.GetPartyByIdAsync(userId, partyId);
        if (party == null) return false;

        _logger.LogInformation("Reminder sent to party {PartyId} by user {UserId}", partyId, userId);
        return true;
    }
}
