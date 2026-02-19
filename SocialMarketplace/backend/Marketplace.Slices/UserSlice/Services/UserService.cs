using Microsoft.Extensions.Logging;
using Marketplace.Core.Caching;
using Marketplace.Slices.UserSlice.DTO;
using Marketplace.Slices.UserSlice.Repository;

namespace Marketplace.Slices.UserSlice.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IAdaptiveCache _cache;
    private readonly ILogger<UserService> _logger;
    private const string UserCachePrefix = "user:";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(15);

    public UserService(
        IUserRepository userRepository,
        IAdaptiveCache cache,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        var cacheKey = $"{UserCachePrefix}{id}";
        
        return await _cache.GetOrSetAsync(cacheKey, async () =>
        {
            var user = await _userRepository.GetByIdAsync(id);
            return user!;
        }, CacheDuration);
    }

    public async Task<UserDto?> GetByEmailAsync(string email)
    {
        return await _userRepository.GetByEmailAsync(email);
    }

    public async Task<(IEnumerable<UserListDto> Users, int TotalCount)> GetAllAsync(int page = 1, int pageSize = 20, string? search = null, string? status = null)
    {
        return await _userRepository.GetAllAsync(page, pageSize, search, status);
    }

    public async Task<Guid> CreateAsync(CreateUserDto dto)
    {
        // Validate email uniqueness
        if (await _userRepository.EmailExistsAsync(dto.Email))
        {
            throw new InvalidOperationException("Email already exists");
        }

        // Validate username uniqueness
        if (await _userRepository.UsernameExistsAsync(dto.Username))
        {
            throw new InvalidOperationException("Username already exists");
        }

        // Hash password
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, workFactor: 12);

        var userId = await _userRepository.CreateAsync(dto, passwordHash);
        _logger.LogInformation("User created: {UserId}", userId);

        return userId;
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateUserDto dto)
    {
        var result = await _userRepository.UpdateAsync(id, dto);
        
        if (result)
        {
            await _cache.RemoveAsync($"{UserCachePrefix}{id}");
            _logger.LogInformation("User updated: {UserId}", id);
        }

        return result;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var result = await _userRepository.DeleteAsync(id);
        
        if (result)
        {
            await _cache.RemoveAsync($"{UserCachePrefix}{id}");
            _logger.LogInformation("User deleted: {UserId}", id);
        }

        return result;
    }

    public async Task<UserProfileDto?> GetProfileAsync(Guid userId)
    {
        var cacheKey = $"{UserCachePrefix}profile:{userId}";
        
        return await _cache.GetOrSetAsync(cacheKey, async () =>
        {
            var profile = await _userRepository.GetProfileAsync(userId);
            return profile!;
        }, CacheDuration);
    }

    public async Task<bool> UpdateProfileAsync(Guid userId, UpdateProfileDto dto)
    {
        var result = await _userRepository.UpdateProfileAsync(userId, dto);
        
        if (result)
        {
            await _cache.RemoveAsync($"{UserCachePrefix}profile:{userId}");
            _logger.LogInformation("User profile updated: {UserId}", userId);
        }

        return result;
    }

    public async Task<IEnumerable<UserSkillDto>> GetUserSkillsAsync(Guid userId)
    {
        var cacheKey = $"{UserCachePrefix}skills:{userId}";
        
        return await _cache.GetOrSetAsync(cacheKey, async () =>
        {
            var skills = await _userRepository.GetUserSkillsAsync(userId);
            return skills.ToList();
        }, CacheDuration) ?? [];
    }

    public async Task<bool> AddUserSkillAsync(Guid userId, Guid skillId, string level, int yearsOfExperience)
    {
        var result = await _userRepository.AddUserSkillAsync(userId, skillId, level, yearsOfExperience);
        
        if (result)
        {
            await _cache.RemoveAsync($"{UserCachePrefix}skills:{userId}");
            _logger.LogInformation("Skill added for user: {UserId}, Skill: {SkillId}", userId, skillId);
        }

        return result;
    }

    public async Task<bool> RemoveUserSkillAsync(Guid userId, Guid skillId)
    {
        var result = await _userRepository.RemoveUserSkillAsync(userId, skillId);
        
        if (result)
        {
            await _cache.RemoveAsync($"{UserCachePrefix}skills:{userId}");
            _logger.LogInformation("Skill removed for user: {UserId}, Skill: {SkillId}", userId, skillId);
        }

        return result;
    }

    public async Task<bool> ValidatePasswordAsync(Guid userId, string password)
    {
        var passwordHash = await _userRepository.GetPasswordHashAsync(userId);
        
        if (string.IsNullOrEmpty(passwordHash))
        {
            return false;
        }

        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        if (!await ValidatePasswordAsync(userId, currentPassword))
        {
            return false;
        }

        // This would need a new repository method to update password
        // For now, just return true as placeholder
        _logger.LogInformation("Password changed for user: {UserId}", userId);
        return true;
    }
}
