using Microsoft.Extensions.Logging;

namespace Marketplace.Slices.ResumeSlice;

public interface IResumeService
{
    Task<ResumeDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<ResumeListDto>> GetMyResumesAsync(Guid userId);
    Task<Guid> CreateAsync(CreateResumeDto dto, Guid userId);
    Task<bool> UpdateAsync(Guid id, Guid userId, UpdateResumeDto dto);
    Task<bool> DeleteAsync(Guid id, Guid userId);
}

public class ResumeService : IResumeService
{
    private readonly IResumeRepository _repository;
    private readonly ILogger<ResumeService> _logger;

    public ResumeService(IResumeRepository repository, ILogger<ResumeService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<ResumeDto?> GetByIdAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<ResumeListDto>> GetMyResumesAsync(Guid userId)
    {
        return await _repository.GetByUserIdAsync(userId);
    }

    public async Task<Guid> CreateAsync(CreateResumeDto dto, Guid userId)
    {
        var id = await _repository.CreateAsync(dto, userId);
        _logger.LogInformation("Resume created: {ResumeId} by user {UserId}", id, userId);
        return id;
    }

    public async Task<bool> UpdateAsync(Guid id, Guid userId, UpdateResumeDto dto)
    {
        var resume = await _repository.GetByIdAsync(id);
        if (resume == null || resume.UserId != userId)
            return false;

        var result = await _repository.UpdateAsync(id, dto);
        if (result) _logger.LogInformation("Resume updated: {ResumeId}", id);
        return result;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId)
    {
        var resume = await _repository.GetByIdAsync(id);
        if (resume == null || resume.UserId != userId)
            return false;

        return await _repository.DeleteAsync(id);
    }
}
