using System.Security.Claims;
using BlazorChat.Server.Application.Features.Servers.Queries;
using BlazorChat.Shared.DTO;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlazorChat.Server.Controllers;

[Authorize]
[ApiController]
[Route("api/servers")]
public class ServerMembersController(IMediator mediator) : ControllerBase
{
    [HttpGet("{serverId:int}/members")]
    public async Task<ActionResult<List<UserDto>>> GetServerMembers(int serverId)
    {
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        var query = new GetServerMembersQuery(serverId, currentUserId);
        var result = await mediator.Send(query, HttpContext.RequestAborted);

        if (!result.IsSuccess)
        {
            return Forbid(); // Non-members are safely rejected 
        }

        return Ok(result.Value);
    }
}