using Marketplace.Slices.UserSlice.DTO;

namespace Marketplace.Slices.UserSlice.Services;

public interface IUserService
{
    Task<UserDto?> GetByIdAsync(Guid id);
    Task<UserDto?> GetByEmailAsync(string email);
    Task<(IEnumerable<UserListDto> Users, int TotalCount)> GetAllAsync(int page = 1, int pageSize = 20, string? search = null, string? status = null);
    Task<Guid> CreateAsync(CreateUserDto dto);
    Task<bool> UpdateAsync(Guid id, UpdateUserDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<UserProfileDto?> GetProfileAsync(Guid userId);
    Task<bool> UpdateProfileAsync(Guid userId, UpdateProfileDto dto);
    Task<IEnumerable<UserSkillDto>> GetUserSkillsAsync(Guid userId);
    Task<bool> AddUserSkillAsync(Guid userId, Guid skillId, string level, int yearsOfExperience);
    Task<bool> RemoveUserSkillAsync(Guid userId, Guid skillId);
    Task<bool> ValidatePasswordAsync(Guid userId, string password);
    Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
}
