using BlazorChat.Server.Features.Channels;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlazorChat.Server.Controllers;

[ApiController]
[Route("api/channels")]
[Authorize]
public class ChannelsController(IMediator mediator) : ControllerBase
{
    [HttpPost("dm/{friendId:int}")]
    public async Task<IActionResult> GetOrCreateDm(int friendId)
    {
        try 
        {
            var channelId = await mediator.Send(new GetOrCreateDmCommand(friendId));
            return Ok(channelId);
        }
        catch (Exception ex) 
        {
            return NotFound(ex.Message);
        }
    }
}