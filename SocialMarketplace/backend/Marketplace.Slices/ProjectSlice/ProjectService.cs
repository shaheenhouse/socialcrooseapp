using Microsoft.Extensions.Logging;
using Marketplace.Core.Infrastructure;

namespace Marketplace.Slices.ProjectSlice;

public interface IProjectService
{
    Task<ProjectDto?> GetByIdAsync(Guid id);
    Task<(IEnumerable<ProjectListDto> Projects, int TotalCount)> GetAllAsync(ProjectQueryParams query);
    Task<(IEnumerable<ProjectListDto> Projects, int TotalCount)> GetMyProjectsAsync(Guid clientId, int page, int pageSize);
    Task<(IEnumerable<ProjectListDto> Projects, int TotalCount)> GetMyFreelanceProjectsAsync(Guid freelancerId, int page, int pageSize);
    Task<Guid> CreateAsync(CreateProjectDto dto, Guid clientId);
    Task<bool> UpdateAsync(Guid id, Guid clientId, UpdateProjectDto dto);
    Task<IEnumerable<ProjectBidDto>> GetBidsAsync(Guid projectId, int page, int pageSize);
    Task<Guid> SubmitBidAsync(CreateBidDto dto, Guid freelancerId);
    Task<IEnumerable<ProjectMilestoneDto>> GetMilestonesAsync(Guid projectId);
    Task<Guid> CreateMilestoneAsync(CreateMilestoneDto dto, Guid userId);
}

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _repository;
    private readonly IJobQueue _jobQueue;
    private readonly ILogger<ProjectService> _logger;

    public ProjectService(IProjectRepository repository, IJobQueue jobQueue, ILogger<ProjectService> logger)
    {
        _repository = repository;
        _jobQueue = jobQueue;
        _logger = logger;
    }

    public async Task<ProjectDto?> GetByIdAsync(Guid id) => await _repository.GetByIdAsync(id);

    public async Task<(IEnumerable<ProjectListDto> Projects, int TotalCount)> GetAllAsync(ProjectQueryParams query)
        => await _repository.GetAllAsync(query);

    public async Task<(IEnumerable<ProjectListDto> Projects, int TotalCount)> GetMyProjectsAsync(Guid clientId, int page, int pageSize)
        => await _repository.GetByClientAsync(clientId, page, pageSize);

    public async Task<(IEnumerable<ProjectListDto> Projects, int TotalCount)> GetMyFreelanceProjectsAsync(Guid freelancerId, int page, int pageSize)
        => await _repository.GetByFreelancerAsync(freelancerId, page, pageSize);

    public async Task<Guid> CreateAsync(CreateProjectDto dto, Guid clientId)
    {
        var id = await _repository.CreateAsync(dto, clientId);
        _logger.LogInformation("Project created: {ProjectId} by client {ClientId}", id, clientId);
        return id;
    }

    public async Task<bool> UpdateAsync(Guid id, Guid clientId, UpdateProjectDto dto)
    {
        var project = await _repository.GetByIdAsync(id);
        if (project == null || project.ClientId != clientId) return false;
        return await _repository.UpdateAsync(id, dto);
    }

    public async Task<IEnumerable<ProjectBidDto>> GetBidsAsync(Guid projectId, int page, int pageSize)
        => await _repository.GetBidsAsync(projectId, page, pageSize);

    public async Task<Guid> SubmitBidAsync(CreateBidDto dto, Guid freelancerId)
    {
        var id = await _repository.SubmitBidAsync(dto, freelancerId);

        var project = await _repository.GetByIdAsync(dto.ProjectId);
        if (project != null)
        {
            await _jobQueue.EnqueueAsync("notifications", new Dictionary<string, string>
            {
                ["Type"] = "new_bid",
                ["ProjectId"] = dto.ProjectId.ToString(),
                ["FreelancerId"] = freelancerId.ToString(),
                ["ClientId"] = project.ClientId.ToString()
            });
        }

        _logger.LogInformation("Bid submitted: {BidId} for project {ProjectId}", id, dto.ProjectId);
        return id;
    }

    public async Task<IEnumerable<ProjectMilestoneDto>> GetMilestonesAsync(Guid projectId)
        => await _repository.GetMilestonesAsync(projectId);

    public async Task<Guid> CreateMilestoneAsync(CreateMilestoneDto dto, Guid userId)
    {
        var project = await _repository.GetByIdAsync(dto.ProjectId);
        if (project == null || project.ClientId != userId)
            throw new UnauthorizedAccessException("Only the project client can create milestones");

        return await _repository.CreateMilestoneAsync(dto);
    }
}
