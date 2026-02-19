using Marketplace.Database.Entities.Social;
using Marketplace.Slices.Social.Follows;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Marketplace.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FollowsController : ControllerBase
{
    private readonly IFollowService _followService;

    public FollowsController(IFollowService followService)
    {
        _followService = followService;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>
    /// Get follow stats for the current user
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetMyStats()
    {
        var stats = await _followService.GetFollowStatsAsync(GetUserId());
        return Ok(stats);
    }

    /// <summary>
    /// Get follow stats for a user
    /// </summary>
    [HttpGet("stats/{userId:guid}")]
    public async Task<IActionResult> GetUserStats(Guid userId)
    {
        var stats = await _followService.GetFollowStatsAsync(userId);
        return Ok(stats);
    }

    /// <summary>
    /// Check if following a target
    /// </summary>
    [HttpGet("status")]
    public async Task<IActionResult> GetFollowStatus(
        [FromQuery] Guid targetId,
        [FromQuery] FollowTargetType targetType = FollowTargetType.User)
    {
        var status = await _followService.GetFollowStatusAsync(GetUserId(), targetId, targetType);
        return Ok(status);
    }

    /// <summary>
    /// Get followers of a target (user, page, store, etc.)
    /// </summary>
    [HttpGet("followers")]
    public async Task<IActionResult> GetFollowers(
        [FromQuery] Guid targetId,
        [FromQuery] FollowTargetType targetType = FollowTargetType.User,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var followers = await _followService.GetFollowersAsync(targetId, targetType, page, pageSize);
        return Ok(followers);
    }

    /// <summary>
    /// Get what the current user is following
    /// </summary>
    [HttpGet("following")]
    public async Task<IActionResult> GetFollowing(
        [FromQuery] FollowTargetType? targetType,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var following = await _followService.GetFollowingAsync(GetUserId(), targetType, page, pageSize);
        return Ok(following);
    }

    /// <summary>
    /// Get what a user is following
    /// </summary>
    [HttpGet("following/{userId:guid}")]
    public async Task<IActionResult> GetUserFollowing(
        Guid userId,
        [FromQuery] FollowTargetType? targetType,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var following = await _followService.GetFollowingAsync(userId, targetType, page, pageSize);
        return Ok(following);
    }

    /// <summary>
    /// Follow a target
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Follow([FromBody] FollowRequest request)
    {
        try
        {
            var followId = await _followService.FollowAsync(GetUserId(), request.TargetId, request.TargetType);
            return Ok(new { FollowId = followId });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Unfollow a target
    /// </summary>
    [HttpDelete]
    public async Task<IActionResult> Unfollow(
        [FromQuery] Guid targetId,
        [FromQuery] FollowTargetType targetType = FollowTargetType.User)
    {
        try
        {
            await _followService.UnfollowAsync(GetUserId(), targetId, targetType);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Toggle notifications for a follow
    /// </summary>
    [HttpPatch("notifications")]
    public async Task<IActionResult> ToggleNotifications([FromBody] ToggleNotificationsRequest request)
    {
        try
        {
            await _followService.ToggleNotificationsAsync(
                GetUserId(), request.TargetId, request.TargetType, request.Enabled);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }
}

public record FollowRequest
{
    public Guid TargetId { get; init; }
    public FollowTargetType TargetType { get; init; } = FollowTargetType.User;
}

public record ToggleNotificationsRequest
{
    public Guid TargetId { get; init; }
    public FollowTargetType TargetType { get; init; } = FollowTargetType.User;
    public bool Enabled { get; init; }
}
