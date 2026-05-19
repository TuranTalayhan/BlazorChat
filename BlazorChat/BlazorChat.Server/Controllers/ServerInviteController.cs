using System.Security.Claims;
using BlazorChat.Server.Application.Common;
using BlazorChat.Server.Application.Features.Servers.Commands;
using BlazorChat.Server.Application.Features.Servers.Queries;
using BlazorChat.Shared.DTO;
using BlazorChat.Shared.Enums;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlazorChat.Server.Controllers;

[Authorize]
[ApiController]
[Route("api/servers")]
public class ServerInviteController(IMediator mediator) : ControllerBase
{
    [HttpPost("{serverId:int}/invites")]
    public async Task<ActionResult<InviteResponseDto>> CreateInvite(int serverId, [FromBody] CreateInviteDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        var command = new CreateInviteCommand(serverId, userId, dto);
        var result = await mediator.Send(command, HttpContext.RequestAborted);

        if (!result.IsSuccess)
        {
            return result.ErrorType == ErrorType.Forbidden ? Forbid() : BadRequest(result.ErrorMessage);
        }

        return Ok(result.Value);
    }

    [HttpPost("join/{code}")]
    public async Task<IActionResult> JoinServer(string code)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        var command = new JoinServerCommand(code, userId);
        var result = await mediator.Send(command, HttpContext.RequestAborted);

        if (!result.IsSuccess)
        {
            return BadRequest(result.ErrorMessage);
        }

        return Ok(result.Value);
    }
    
    [HttpGet("{serverId:int}/role")]
    public async Task<ActionResult<ServerRole>> GetUserRole(int serverId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        var query = new GetUserRoleQuery(serverId, userId);
        var role = await mediator.Send(query, HttpContext.RequestAborted);
        
        return Ok(role);
    }
}