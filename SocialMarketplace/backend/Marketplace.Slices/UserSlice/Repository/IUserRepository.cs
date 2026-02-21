using Marketplace.Slices.UserSlice.DTO;

namespace Marketplace.Slices.UserSlice.Repository;

public interface IUserRepository
{
    Task<UserDto?> GetByIdAsync(Guid id);
    Task<UserDto?> GetByEmailAsync(string email);
    Task<UserDto?> GetByUsernameAsync(string username);
    Task<(IEnumerable<UserListDto> Users, int TotalCount)> GetAllAsync(int page, int pageSize, string? search = null, string? status = null);
    Task<Guid> CreateAsync(CreateUserDto dto, string passwordHash);
    Task<bool> UpdateAsync(Guid id, UpdateUserDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> UpdateLastLoginAsync(Guid id, string ipAddress);
    Task<string?> GetPasswordHashAsync(Guid id);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> UsernameExistsAsync(string username);
    
    // Profile
    Task<UserProfileDto?> GetProfileAsync(Guid userId);
    Task<bool> UpdateProfileAsync(Guid userId, UpdateProfileDto dto);
    
    // Roles
    Task<IEnumerable<string>> GetUserRolesAsync(Guid userId);
    Task<bool> AssignDefaultRoleAsync(Guid userId);
    
    // Skills
    Task<IEnumerable<UserSkillDto>> GetUserSkillsAsync(Guid userId);
    Task<bool> AddUserSkillAsync(Guid userId, Guid skillId, string level, int yearsOfExperience);
    Task<bool> RemoveUserSkillAsync(Guid userId, Guid skillId);
}
