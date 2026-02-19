using Marketplace.Database.Entities.Social;
using Marketplace.Slices.Social.Connections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Marketplace.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ConnectionsController : ControllerBase
{
    private readonly IConnectionService _connectionService;

    public ConnectionsController(IConnectionService connectionService)
    {
        _connectionService = connectionService;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>
    /// Get my connections
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMyConnections(
        [FromQuery] ConnectionStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _connectionService.GetMyConnectionsAsync(GetUserId(), status, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Get pending connection requests received
    /// </summary>
    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingRequests(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _connectionService.GetPendingRequestsAsync(GetUserId(), page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Get sent connection requests
    /// </summary>
    [HttpGet("sent")]
    public async Task<IActionResult> GetSentRequests(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _connectionService.GetSentRequestsAsync(GetUserId(), page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Get connection stats
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var stats = await _connectionService.GetConnectionStatsAsync(GetUserId());
        return Ok(stats);
    }

    /// <summary>
    /// Get connection suggestions (people you may know)
    /// </summary>
    [HttpGet("suggestions")]
    public async Task<IActionResult> GetSuggestions([FromQuery] int limit = 20)
    {
        var suggestions = await _connectionService.GetSuggestionsAsync(GetUserId(), limit);
        return Ok(suggestions);
    }

    /// <summary>
    /// Check connection status with a user
    /// </summary>
    [HttpGet("status/{userId:guid}")]
    public async Task<IActionResult> GetConnectionStatus(Guid userId)
    {
        var status = await _connectionService.GetConnectionStatusAsync(GetUserId(), userId);
        return Ok(status);
    }

    /// <summary>
    /// Send a connection request
    /// </summary>
    [HttpPost("request")]
    public async Task<IActionResult> SendConnectionRequest([FromBody] SendConnectionRequest request)
    {
        try
        {
            var connectionId = await _connectionService.SendConnectionRequestAsync(
                GetUserId(), request.UserId, request.Message);
            return Ok(new { ConnectionId = connectionId });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Accept a connection request
    /// </summary>
    [HttpPost("{connectionId:guid}/accept")]
    public async Task<IActionResult> AcceptConnection(Guid connectionId)
    {
        try
        {
            await _connectionService.AcceptConnectionAsync(connectionId, GetUserId());
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Reject a connection request
    /// </summary>
    [HttpPost("{connectionId:guid}/reject")]
    public async Task<IActionResult> RejectConnection(Guid connectionId)
    {
        try
        {
            await _connectionService.RejectConnectionAsync(connectionId, GetUserId());
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Withdraw a sent connection request
    /// </summary>
    [HttpPost("{connectionId:guid}/withdraw")]
    public async Task<IActionResult> WithdrawConnection(Guid connectionId)
    {
        try
        {
            await _connectionService.WithdrawConnectionAsync(connectionId, GetUserId());
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Remove a connection
    /// </summary>
    [HttpDelete("{connectionId:guid}")]
    public async Task<IActionResult> RemoveConnection(Guid connectionId)
    {
        try
        {
            await _connectionService.RemoveConnectionAsync(connectionId, GetUserId());
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Block a user
    /// </summary>
    [HttpPost("block/{userId:guid}")]
    public async Task<IActionResult> BlockUser(Guid userId)
    {
        await _connectionService.BlockUserAsync(GetUserId(), userId);
        return Ok();
    }
}

public record SendConnectionRequest
{
    public Guid UserId { get; init; }
    public string? Message { get; init; }
}
