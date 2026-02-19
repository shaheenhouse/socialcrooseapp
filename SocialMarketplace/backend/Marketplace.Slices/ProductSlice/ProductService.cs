using Microsoft.Extensions.Logging;
using Marketplace.Core.Caching;

namespace Marketplace.Slices.ProductSlice;

public interface IProductService
{
    Task<ProductDto?> GetByIdAsync(Guid id);
    Task<ProductDto?> GetBySlugAsync(string slug);
    Task<(IEnumerable<ProductListDto> Products, int TotalCount)> GetAllAsync(ProductQueryParams query);
    Task<IEnumerable<ProductListDto>> GetByStoreIdAsync(Guid storeId, int page, int pageSize);
    Task<Guid> CreateAsync(CreateProductDto dto, Guid storeId);
    Task<bool> UpdateAsync(Guid id, UpdateProductDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<ProductImageDto>> GetImagesAsync(Guid productId);
    Task<IEnumerable<ProductVariantDto>> GetVariantsAsync(Guid productId);
    Task<IEnumerable<ProductReviewDto>> GetReviewsAsync(Guid productId, int page, int pageSize);
}

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    private readonly IAdaptiveCache _cache;
    private readonly ILogger<ProductService> _logger;
    private const string CachePrefix = "product:";

    public ProductService(IProductRepository repository, IAdaptiveCache cache, ILogger<ProductService> logger)
    {
        _repository = repository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id)
    {
        return await _cache.GetOrSetAsync($"{CachePrefix}{id}", async () =>
        {
            return (await _repository.GetByIdAsync(id))!;
        }, TimeSpan.FromMinutes(10));
    }

    public async Task<ProductDto?> GetBySlugAsync(string slug)
    {
        return await _repository.GetBySlugAsync(slug);
    }

    public async Task<(IEnumerable<ProductListDto> Products, int TotalCount)> GetAllAsync(ProductQueryParams query)
    {
        return await _repository.GetAllAsync(query);
    }

    public async Task<IEnumerable<ProductListDto>> GetByStoreIdAsync(Guid storeId, int page, int pageSize)
    {
        return await _repository.GetByStoreIdAsync(storeId, page, pageSize);
    }

    public async Task<Guid> CreateAsync(CreateProductDto dto, Guid storeId)
    {
        var id = await _repository.CreateAsync(dto, storeId);
        _logger.LogInformation("Product created: {ProductId} in store {StoreId}", id, storeId);
        return id;
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateProductDto dto)
    {
        var result = await _repository.UpdateAsync(id, dto);
        if (result)
        {
            await _cache.RemoveAsync($"{CachePrefix}{id}");
            _logger.LogInformation("Product updated: {ProductId}", id);
        }
        return result;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var result = await _repository.DeleteAsync(id);
        if (result) await _cache.RemoveAsync($"{CachePrefix}{id}");
        return result;
    }

    public async Task<IEnumerable<ProductImageDto>> GetImagesAsync(Guid productId) => await _repository.GetImagesAsync(productId);
    public async Task<IEnumerable<ProductVariantDto>> GetVariantsAsync(Guid productId) => await _repository.GetVariantsAsync(productId);
    public async Task<IEnumerable<ProductReviewDto>> GetReviewsAsync(Guid productId, int page, int pageSize) => await _repository.GetReviewsAsync(productId, page, pageSize);
}
