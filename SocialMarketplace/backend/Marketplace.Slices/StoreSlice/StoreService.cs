using Microsoft.Extensions.Logging;
using Marketplace.Core.Caching;

namespace Marketplace.Slices.StoreSlice;

public interface IStoreService
{
    Task<StoreDto?> GetByIdAsync(Guid id);
    Task<StoreDto?> GetBySlugAsync(string slug);
    Task<StoreDto?> GetMyStoreAsync(Guid ownerId);
    Task<(IEnumerable<StoreListDto> Stores, int TotalCount)> GetAllAsync(int page, int pageSize, string? search = null, string? status = null);
    Task<Guid> CreateAsync(CreateStoreDto dto, Guid ownerId);
    Task<bool> UpdateAsync(Guid id, Guid ownerId, UpdateStoreDto dto);
    Task<bool> DeleteAsync(Guid id, Guid ownerId);
    Task<IEnumerable<StoreEmployeeDto>> GetEmployeesAsync(Guid storeId);
    Task<StoreAnalyticsDto> GetAnalyticsAsync(Guid storeId, Guid ownerId);
}

public class StoreService : IStoreService
{
    private readonly IStoreRepository _repository;
    private readonly IAdaptiveCache _cache;
    private readonly ILogger<StoreService> _logger;
    private const string CachePrefix = "store:";

    public StoreService(IStoreRepository repository, IAdaptiveCache cache, ILogger<StoreService> logger)
    {
        _repository = repository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<StoreDto?> GetByIdAsync(Guid id)
    {
        return await _cache.GetOrSetAsync($"{CachePrefix}{id}", async () =>
        {
            return (await _repository.GetByIdAsync(id))!;
        }, TimeSpan.FromMinutes(10));
    }

    public async Task<StoreDto?> GetBySlugAsync(string slug)
    {
        return await _repository.GetBySlugAsync(slug);
    }

    public async Task<StoreDto?> GetMyStoreAsync(Guid ownerId)
    {
        return await _repository.GetByOwnerIdAsync(ownerId);
    }

    public async Task<(IEnumerable<StoreListDto> Stores, int TotalCount)> GetAllAsync(int page, int pageSize, string? search = null, string? status = null)
    {
        return await _repository.GetAllAsync(page, pageSize, search, status);
    }

    public async Task<Guid> CreateAsync(CreateStoreDto dto, Guid ownerId)
    {
        var existing = await _repository.GetByOwnerIdAsync(ownerId);
        if (existing != null)
            throw new InvalidOperationException("User already has a store");

        var id = await _repository.CreateAsync(dto, ownerId);
        _logger.LogInformation("Store created: {StoreId} by user {UserId}", id, ownerId);
        return id;
    }

    public async Task<bool> UpdateAsync(Guid id, Guid ownerId, UpdateStoreDto dto)
    {
        var store = await _repository.GetByIdAsync(id);
        if (store == null || store.OwnerId != ownerId)
            return false;

        var result = await _repository.UpdateAsync(id, dto);
        if (result)
        {
            await _cache.RemoveAsync($"{CachePrefix}{id}");
            _logger.LogInformation("Store updated: {StoreId}", id);
        }
        return result;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid ownerId)
    {
        var store = await _repository.GetByIdAsync(id);
        if (store == null || store.OwnerId != ownerId)
            return false;

        var result = await _repository.DeleteAsync(id);
        if (result) await _cache.RemoveAsync($"{CachePrefix}{id}");
        return result;
    }

    public async Task<IEnumerable<StoreEmployeeDto>> GetEmployeesAsync(Guid storeId)
    {
        return await _repository.GetEmployeesAsync(storeId);
    }

    public async Task<StoreAnalyticsDto> GetAnalyticsAsync(Guid storeId, Guid ownerId)
    {
        var store = await _repository.GetByIdAsync(storeId);
        if (store == null || store.OwnerId != ownerId)
            throw new UnauthorizedAccessException("Not the store owner");

        return await _repository.GetAnalyticsAsync(storeId);
    }
}
