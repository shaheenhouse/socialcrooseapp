using Microsoft.Extensions.Logging;

namespace Marketplace.Slices.DesignSlice;

public interface IDesignService
{
    Task<DesignDto?> GetByIdAsync(Guid id);
    Task<(IEnumerable<DesignListDto> Designs, int TotalCount)> GetMyDesignsAsync(Guid userId, int page, int pageSize);
    Task<(IEnumerable<DesignListDto> Templates, int TotalCount)> GetTemplatesAsync(int page, int pageSize, string? category = null);
    Task<Guid> CreateAsync(CreateDesignDto dto, Guid userId);
    Task<Guid> DuplicateAsync(Guid sourceId, Guid userId);
    Task<bool> UpdateAsync(Guid id, Guid userId, UpdateDesignDto dto);
    Task<bool> DeleteAsync(Guid id, Guid userId);
}

public class DesignService : IDesignService
{
    private readonly IDesignRepository _repository;
    private readonly ILogger<DesignService> _logger;

    public DesignService(IDesignRepository repository, ILogger<DesignService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<DesignDto?> GetByIdAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<(IEnumerable<DesignListDto> Designs, int TotalCount)> GetMyDesignsAsync(Guid userId, int page, int pageSize)
    {
        var designs = await _repository.GetByUserIdAsync(userId, page, pageSize);
        var totalCount = await _repository.GetCountByUserIdAsync(userId);
        return (designs, totalCount);
    }

    public async Task<(IEnumerable<DesignListDto> Templates, int TotalCount)> GetTemplatesAsync(int page, int pageSize, string? category = null)
    {
        var templates = await _repository.GetTemplatesAsync(page, pageSize, category);
        var totalCount = await _repository.GetTemplateCountAsync(category);
        return (templates, totalCount);
    }

    public async Task<Guid> CreateAsync(CreateDesignDto dto, Guid userId)
    {
        var id = await _repository.CreateAsync(dto, userId);
        _logger.LogInformation("Design created: {DesignId} by user {UserId}", id, userId);
        return id;
    }

    public async Task<Guid> DuplicateAsync(Guid sourceId, Guid userId)
    {
        var source = await _repository.GetByIdAsync(sourceId);
        if (source == null)
            throw new InvalidOperationException("Source design not found");

        var dto = new CreateDesignDto(
            Name: $"{source.Name} (Copy)",
            Description: source.Description,
            Width: source.Width,
            Height: source.Height,
            CanvasJson: source.CanvasJson,
            Thumbnail: source.Thumbnail,
            Category: source.Category,
            Tags: source.Tags
        );

        var id = await _repository.CreateAsync(dto, userId);
        _logger.LogInformation("Design duplicated: {SourceId} -> {NewId} by user {UserId}", sourceId, id, userId);
        return id;
    }

    public async Task<bool> UpdateAsync(Guid id, Guid userId, UpdateDesignDto dto)
    {
        var design = await _repository.GetByIdAsync(id);
        if (design == null || design.UserId != userId)
            return false;

        var result = await _repository.UpdateAsync(id, dto);
        if (result) _logger.LogInformation("Design updated: {DesignId}", id);
        return result;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId)
    {
        var design = await _repository.GetByIdAsync(id);
        if (design == null || design.UserId != userId)
            return false;

        return await _repository.DeleteAsync(id);
    }
}
