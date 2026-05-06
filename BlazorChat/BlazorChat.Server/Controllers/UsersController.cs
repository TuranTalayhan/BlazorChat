using System.Security.Claims;
using Mediator;
using BlazorChat.Server.Application.Features.Users.Commands;
using BlazorChat.Server.Application.Features.Users.Queries;
using BlazorChat.Shared.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlazorChat.Server.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController(IMediator mediator) : ControllerBase
{
    private int GetCurrentUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(idClaim, out var id) ? id : 0;
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q = "", CancellationToken ct = default)
    {
        var currentId = GetCurrentUserId();
        if (currentId == 0) return Unauthorized();

        var users = await mediator.Send(new SearchUsersQuery(currentId, q), ct);
        return Ok(users);
    }
    
    [HttpGet("me/status")]
    public async Task<IActionResult> GetStatus(CancellationToken ct = default)
    {
        var currentId = GetCurrentUserId();
        if (currentId == 0) return Unauthorized();

        var status = await mediator.Send(new GetUserStatusQuery(currentId), ct);
        
        if (status == null) return NotFound();
        
        return Ok(status);
    }

    [HttpPatch("me/status")]
    public async Task<IActionResult> UpdateStatus([FromBody] UpdateStatusDto dto, CancellationToken ct = default)
    {
        var currentId = GetCurrentUserId();
        if (currentId == 0) return Unauthorized();

        var success = await mediator.Send(new UpdateUserStatusCommand(currentId, dto), ct);

        if (!success) return NotFound();

        return Ok();
    }
}