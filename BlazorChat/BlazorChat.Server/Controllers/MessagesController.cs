using System.Security.Claims;
using Mediator;
using BlazorChat.Server.Application.Features.Messages;
using BlazorChat.Server.Application.Features.Messages.Commands;
using BlazorChat.Server.Application.Features.Messages.Queries;
using BlazorChat.Shared.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlazorChat.Server.Controllers;

[ApiController]
[Route("api/messages")]
[Authorize]
public class MessagesController(IMediator mediator) : ControllerBase
{
    private int GetCurrentUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(idClaim, out var id) ? id : 0;
    }

    [HttpGet("{channelId:int}")]
    public async Task<IActionResult> GetMessages(
        int channelId, 
        [FromQuery] int count = 50, 
        [FromQuery] DateTime? before = null, 
        [FromQuery] int? messageId = null,
        CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        if (userId == 0) return Unauthorized();

        var result = await mediator.Send(new GetMessagesQuery(userId, channelId, count, before, messageId), ct);

        if (!result.IsSuccess)
        {
            return result.Error switch
            {
                MessageError.Forbidden => Forbid(),
                _ => BadRequest(new { message = result.ErrorMessage })
            };
        }

        return Ok(result.Data);
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageDto dto, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId == 0) return Unauthorized();

        var result = await mediator.Send(new SendMessageCommand(userId, dto), ct);

        if (!result.IsSuccess)
        {
            return result.Error switch
            {
                MessageError.Forbidden => Forbid(),
                MessageError.NotFound => NotFound(new { message = result.ErrorMessage }),
                MessageError.BadRequest => BadRequest(new { message = result.ErrorMessage }),
                _ => StatusCode(500)
            };
        }

        return Ok(result.Data);
    }
    
    [HttpGet("unread-states")]
    public async Task<IActionResult> GetUnreadStatuses(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId == 0) return Unauthorized();

        var result = await mediator.Send(new GetUnreadStatusesQuery(userId), ct);
        
        return Ok(result);
    }
}