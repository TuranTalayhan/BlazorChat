using System.Security.Claims;
using BlazorChat.Server.Application.Features.Channels;
using BlazorChat.Server.Application.Features.Channels.Commands;
using BlazorChat.Server.Application.Features.Channels.Queries;
using BlazorChat.Shared.DTO;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlazorChat.Server.Controllers;

[ApiController]
[Route("api/channels")]
[Authorize]
public class ChannelsController(IMediator mediator) : ControllerBase
{
    private int GetCurrentUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(idClaim, out var id) ? id : 0;
    }
    
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetChannel(int id, CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        if (userId == 0) return Unauthorized();

        var result = await mediator.Send(new GetChannelByIdQuery(userId, id), ct);

        if (!result.IsSuccess)
        {
            return result.Error switch
            {
                ChannelError.Forbidden => Forbid(),
                ChannelError.NotFound => NotFound(),
                _ => BadRequest()
            };
        }

        return Ok(result.Data);
    }
    
    [HttpPatch("{id:int}")]
    public async Task<IActionResult> UpdateChannel(int id, [FromBody] UpdateChannelDto dto, CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        if (userId == 0) return Unauthorized();

        var result = await mediator.Send(new UpdateChannelCommand(userId, id, dto), ct);

        if (!result.IsSuccess)
        {
            return result.Error switch
            {
                ChannelError.Forbidden => Forbid(),
                ChannelError.NotFound => NotFound(),
                ChannelError.BadRequest => BadRequest(new { message = result.ErrorMessage }),
                _ => StatusCode(500)
            };
        }

        return Ok();
    }
    
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteChannel(int id, CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        if (userId == 0) return Unauthorized();

        var result = await mediator.Send(new DeleteChannelCommand(userId, id), ct);

        if (!result.IsSuccess)
        {
            return result.Error switch
            {
                ChannelError.Forbidden => Forbid(),
                ChannelError.NotFound => NotFound(),
                _ => BadRequest()
            };
        }

        return NoContent();
    }
}