using System.Security.Claims;
using Mediator;
using BlazorChat.Server.Application.Features.Channels;
using BlazorChat.Server.Application.Features.Channels.Commands;
using BlazorChat.Server.Application.Features.Channels.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlazorChat.Server.Controllers;

[ApiController]
[Route("api/dms")]
[Authorize]
public class DirectMessagesController(IMediator mediator) : ControllerBase
{
    private int GetCurrentUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(idClaim, out var id) ? id : 0;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetMyDirectMessages(CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        if (userId == 0) return Unauthorized();

        var dms = await mediator.Send(new GetMyDmsQuery(userId), ct);
        return Ok(dms);
    }

    // POST /api/dms
    // The "Intent to Converse" (Get or Create) endpoint
    [HttpPost]
    public async Task<IActionResult> OpenDirectMessage([FromBody] int friendId, CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        if (userId == 0) return Unauthorized();

        var result = await mediator.Send(new OpenDmCommand(userId, friendId), ct);

        if (!result.IsSuccess)
        {
            return result.Error switch
            {
                ChannelError.NotFound => NotFound(new { message = result.ErrorMessage }),
                ChannelError.BadRequest => BadRequest(new { message = result.ErrorMessage }),
                _ => StatusCode(500)
            };
        }

        // If it's a brand new DM channel, return 201 Created
        if (result.IsNewChannel)
        {
            // Point to the ChannelsController to fetch the actual channel data
            return CreatedAtAction(
                actionName: nameof(ChannelsController.GetChannel), 
                controllerName: "Channels", 
                routeValues: new { id = result.Data }, 
                value: result.Data);
        }

        // If the DM already existed, return 200 OK
        return Ok(result.Data);
    }
}