using System.Security.Claims;
using Mediator;
using BlazorChat.Server.Application.Features.Friendships;
using BlazorChat.Server.Application.Features.Friendships.Commands;
using BlazorChat.Server.Application.Features.Friendships.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlazorChat.Server.Controllers;

[ApiController]
[Route("api/friendships")]
[Authorize]
public class FriendshipsController(IMediator mediator) : ControllerBase
{
    private int GetCurrentUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(idClaim, out var id) ? id : 0;
    }

    [HttpGet]
    public async Task<IActionResult> GetFriends(CancellationToken ct)
    {
        var currentId = GetCurrentUserId();
        if (currentId == 0) return Unauthorized();

        var friends = await mediator.Send(new GetFriendsQuery(currentId), ct);
        return Ok(friends);
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingRequests(CancellationToken ct)
    {
        var currentId = GetCurrentUserId();
        if (currentId == 0) return Unauthorized();

        var pending = await mediator.Send(new GetPendingRequestsQuery(currentId), ct);
        return Ok(pending);
    }

    [HttpPost]
    public async Task<IActionResult> SendRequest([FromBody] string targetUsername, CancellationToken ct)
    {
        var currentId = GetCurrentUserId();
        if (currentId == 0) return Unauthorized();

        var result = await mediator.Send(new SendFriendRequestCommand(currentId, targetUsername), ct);

        return result.Error switch
        {
            FriendshipError.NotFound => NotFound(new { message = result.ErrorMessage }),
            FriendshipError.BadRequest => BadRequest(new { message = result.ErrorMessage }),
            FriendshipError.Conflict => Conflict(new { message = result.ErrorMessage }),
            _ => Ok()
        };
    }

    [HttpPatch("{requesterId:int}")]
    public async Task<IActionResult> RespondToRequest(int requesterId, [FromBody] bool accept, CancellationToken ct)
    {
        var currentId = GetCurrentUserId();
        if (currentId == 0) return Unauthorized();

        var result = await mediator.Send(new RespondToFriendRequestCommand(currentId, requesterId, accept), ct);

        if (result is { IsSuccess: false, Error: FriendshipError.NotFound })
            return NotFound();

        return Ok();
    }
    
    [HttpGet("sidebar-summary")]
    public async Task<IActionResult> GetSidebarSummary(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId == 0) return Unauthorized();

        var summary = await mediator.Send(new GetFriendsSummaryQuery(userId), ct);

        return Ok(summary);
    }
}