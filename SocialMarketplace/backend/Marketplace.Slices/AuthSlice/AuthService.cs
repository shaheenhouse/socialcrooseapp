using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Marketplace.Core.Caching;
using Marketplace.Slices.UserSlice.Repository;

namespace Marketplace.Slices.AuthSlice;

public interface IAuthService
{
    Task<AuthResult> LoginAsync(LoginDto dto);
    Task<AuthResult> RegisterAsync(RegisterDto dto);
    Task<AuthResult> RefreshTokenAsync(string refreshToken);
    Task<bool> RevokeTokenAsync(Guid userId, string refreshToken);
    Task<bool> RevokeAllTokensAsync(Guid userId);
    string GenerateAccessToken(Guid userId, string email, IEnumerable<string> roles);
    string GenerateRefreshToken();
}

public record LoginDto(string Email, string Password);
public record RegisterDto(string Email, string Username, string Password, string FirstName, string LastName, string? PhoneNumber = null);
public record AuthResult(bool Success, string? AccessToken = null, string? RefreshToken = null, DateTime? ExpiresAt = null, string? Error = null, UserInfo? User = null);
public record UserInfo(Guid Id, string Email, string Username, string FirstName, string LastName, string? AvatarUrl, IEnumerable<string> Roles);

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IAdaptiveCache _cache;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IAdaptiveCache cache,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _cache = cache;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AuthResult> LoginAsync(LoginDto dto)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email);
        if (user == null)
        {
            return new AuthResult(false, Error: "Invalid email or password");
        }

        var passwordHash = await _userRepository.GetPasswordHashAsync(user.Id);
        if (string.IsNullOrEmpty(passwordHash) || !BCrypt.Net.BCrypt.Verify(dto.Password, passwordHash))
        {
            _logger.LogWarning("Failed login attempt for user: {Email}", dto.Email);
            return new AuthResult(false, Error: "Invalid email or password");
        }

        if (user.Status != "Active")
        {
            return new AuthResult(false, Error: "Account is not active");
        }

        var roles = new List<string> { "User" }; // TODO: Get actual roles from database
        var accessToken = GenerateAccessToken(user.Id, user.Email, roles);
        var refreshToken = GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddMinutes(_configuration.GetValue("Jwt:ExpiryMinutes", 60));

        // Store refresh token
        await _cache.SetAsync($"refresh:{refreshToken}", new { UserId = user.Id, CreatedAt = DateTime.UtcNow }, TimeSpan.FromDays(7));

        // Update last login
        await _userRepository.UpdateLastLoginAsync(user.Id, "127.0.0.1");

        _logger.LogInformation("User logged in: {UserId}", user.Id);

        return new AuthResult(
            true,
            accessToken,
            refreshToken,
            expiresAt,
            User: new UserInfo(user.Id, user.Email, user.Username, user.FirstName, user.LastName, user.AvatarUrl, roles)
        );
    }

    public async Task<AuthResult> RegisterAsync(RegisterDto dto)
    {
        // Check if email exists
        if (await _userRepository.EmailExistsAsync(dto.Email))
        {
            return new AuthResult(false, Error: "Email already registered");
        }

        // Check if username exists
        if (await _userRepository.UsernameExistsAsync(dto.Username))
        {
            return new AuthResult(false, Error: "Username already taken");
        }

        // Hash password and create user
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, workFactor: 12);
        var createDto = new UserSlice.DTO.CreateUserDto(dto.Email, dto.Username, dto.Password, dto.FirstName, dto.LastName, dto.PhoneNumber);
        var userId = await _userRepository.CreateAsync(createDto, passwordHash);

        var roles = new List<string> { "User" };
        var accessToken = GenerateAccessToken(userId, dto.Email, roles);
        var refreshToken = GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddMinutes(_configuration.GetValue("Jwt:ExpiryMinutes", 60));

        // Store refresh token
        await _cache.SetAsync($"refresh:{refreshToken}", new { UserId = userId, CreatedAt = DateTime.UtcNow }, TimeSpan.FromDays(7));

        _logger.LogInformation("User registered: {UserId}", userId);

        return new AuthResult(
            true,
            accessToken,
            refreshToken,
            expiresAt,
            User: new UserInfo(userId, dto.Email, dto.Username, dto.FirstName, dto.LastName, null, roles)
        );
    }

    public async Task<AuthResult> RefreshTokenAsync(string refreshToken)
    {
        var cached = await _cache.GetAsync<dynamic>($"refresh:{refreshToken}");
        if (cached == null)
        {
            return new AuthResult(false, Error: "Invalid refresh token");
        }

        var userId = (Guid)cached.UserId;
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return new AuthResult(false, Error: "User not found");
        }

        // Remove old refresh token
        await _cache.RemoveAsync($"refresh:{refreshToken}");

        var roles = new List<string> { "User" };
        var newAccessToken = GenerateAccessToken(userId, user.Email, roles);
        var newRefreshToken = GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddMinutes(_configuration.GetValue("Jwt:ExpiryMinutes", 60));

        // Store new refresh token
        await _cache.SetAsync($"refresh:{newRefreshToken}", new { UserId = userId, CreatedAt = DateTime.UtcNow }, TimeSpan.FromDays(7));

        return new AuthResult(
            true,
            newAccessToken,
            newRefreshToken,
            expiresAt,
            User: new UserInfo(user.Id, user.Email, user.Username, user.FirstName, user.LastName, user.AvatarUrl, roles)
        );
    }

    public async Task<bool> RevokeTokenAsync(Guid userId, string refreshToken)
    {
        await _cache.RemoveAsync($"refresh:{refreshToken}");
        _logger.LogInformation("Refresh token revoked for user: {UserId}", userId);
        return true;
    }

    public async Task<bool> RevokeAllTokensAsync(Guid userId)
    {
        await _cache.RemoveByPrefixAsync($"refresh:");
        _logger.LogInformation("All refresh tokens revoked for user: {UserId}", userId);
        return true;
    }

    public string GenerateAccessToken(Guid userId, string email, IEnumerable<string> roles)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key not configured")));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_configuration.GetValue("Jwt:ExpiryMinutes", 60)),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray()) + Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    }
}
