using Microsoft.Extensions.Logging;
using Marketplace.Core.Caching;

namespace Marketplace.Slices.PortfolioSlice;

public interface IPortfolioService
{
    Task<PortfolioDto?> GetByIdAsync(Guid id);
    Task<PortfolioDto?> GetBySlugAsync(string slug);
    Task<PortfolioDto?> GetMyPortfolioAsync(Guid userId);
    Task<Guid> CreateAsync(CreatePortfolioDto dto, Guid userId);
    Task<bool> UpdateAsync(Guid id, Guid userId, UpdatePortfolioDto dto);
    Task<bool> DeleteAsync(Guid id, Guid userId);
    Task<(IEnumerable<PortfolioListDto> Portfolios, int TotalCount)> GetPublicAsync(int page, int pageSize);
}

public class PortfolioService : IPortfolioService
{
    private readonly IPortfolioRepository _repository;
    private readonly IAdaptiveCache _cache;
    private readonly ILogger<PortfolioService> _logger;
    private const string CachePrefix = "portfolio:";

    public PortfolioService(IPortfolioRepository repository, IAdaptiveCache cache, ILogger<PortfolioService> logger)
    {
        _repository = repository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<PortfolioDto?> GetByIdAsync(Guid id)
    {
        return await _cache.GetOrSetAsync($"{CachePrefix}{id}", async () =>
        {
            return (await _repository.GetByIdAsync(id))!;
        }, TimeSpan.FromMinutes(10));
    }

    public async Task<PortfolioDto?> GetBySlugAsync(string slug)
    {
        return await _repository.GetBySlugAsync(slug);
    }

    public async Task<PortfolioDto?> GetMyPortfolioAsync(Guid userId)
    {
        return await _repository.GetByUserIdAsync(userId);
    }

    public async Task<Guid> CreateAsync(CreatePortfolioDto dto, Guid userId)
    {
        var existing = await _repository.GetByUserIdAsync(userId);
        if (existing != null)
            throw new InvalidOperationException("User already has a portfolio");

        var id = await _repository.CreateAsync(dto, userId);
        _logger.LogInformation("Portfolio created: {PortfolioId} by user {UserId}", id, userId);
        return id;
    }

    public async Task<bool> UpdateAsync(Guid id, Guid userId, UpdatePortfolioDto dto)
    {
        var portfolio = await _repository.GetByIdAsync(id);
        if (portfolio == null || portfolio.UserId != userId)
            return false;

        var result = await _repository.UpdateAsync(id, dto);
        if (result)
        {
            await _cache.RemoveAsync($"{CachePrefix}{id}");
            _logger.LogInformation("Portfolio updated: {PortfolioId}", id);
        }
        return result;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId)
    {
        var portfolio = await _repository.GetByIdAsync(id);
        if (portfolio == null || portfolio.UserId != userId)
            return false;

        var result = await _repository.DeleteAsync(id);
        if (result) await _cache.RemoveAsync($"{CachePrefix}{id}");
        return result;
    }

    public async Task<(IEnumerable<PortfolioListDto> Portfolios, int TotalCount)> GetPublicAsync(int page, int pageSize)
    {
        var portfolios = await _repository.GetPublicPortfoliosAsync(page, pageSize);
        var totalCount = await _repository.GetPublicCountAsync();
        return (portfolios, totalCount);
    }
}
