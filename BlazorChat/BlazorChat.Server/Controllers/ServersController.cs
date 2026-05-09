using System.Security.Claims;
using BlazorChat.Server.Application.Features.ChannelCategories.Commands;
using BlazorChat.Server.Application.Features.Channels;
using BlazorChat.Server.Application.Features.Channels.Commands;
using Mediator;
using BlazorChat.Server.Application.Features.Servers;
using BlazorChat.Server.Application.Features.Servers.Commands;
using BlazorChat.Server.Application.Features.Servers.Queries;
using BlazorChat.Shared.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlazorChat.Server.Controllers;

[ApiController]
[Route("api/servers")]
[Authorize]
public class ServersController(IMediator mediator) : ControllerBase
{
    private int GetCurrentUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(idClaim, out var id) ? id : 0;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyServers(CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        if (userId == 0) return Unauthorized();

        var servers = await mediator.Send(new GetMyServersQuery(userId), ct);
        return Ok(servers);
    }

    [HttpPost]
    public async Task<IActionResult> CreateServer([FromBody] CreateServerDto dto, CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        if (userId == 0) return Unauthorized();

        var result = await mediator.Send(new CreateServerCommand(userId, dto), ct);
        
        return CreatedAtAction(nameof(GetServer), new { id = result.Id }, result);
    }
    
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetServer(int id, CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        if (userId == 0) return Unauthorized();

        var result = await mediator.Send(new GetServerByIdQuery(userId, id), ct);

        if (!result.IsSuccess)
        {
            return result.Error switch
            {
                ServerError.Forbidden => Forbid(),
                ServerError.NotFound => NotFound(),
                _ => BadRequest()
            };
        }

        return Ok(result.Data);
    }

    [HttpGet("{id:int}/channels")]
    public async Task<IActionResult> GetChannels(int id, CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        if (userId == 0) return Unauthorized();

        var result = await mediator.Send(new GetServerChannelsQuery(userId, id), ct);

        if (!result.IsSuccess)
        {
            return result.Error switch
            {
                ServerError.Forbidden => Forbid(),
                _ => BadRequest()
            };
        }

        return Ok(result.Data);
    }
    
    [HttpPost("{serverId:int}/channels")]
    public async Task<IActionResult> CreateChannel(int serverId, [FromBody] CreateServerChannelDto dto, CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        if (userId == 0) return Unauthorized();

        var result = await mediator.Send(new CreateServerChannelCommand(userId, serverId, dto.Name, dto.CategoryName, dto.CategoryId), ct);

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

        return Ok(result.Data);
    }

    [HttpGet("{serverId:int}/categories")]
    public async Task<IActionResult> GetCategories(int serverId, CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        if (userId == 0) return Unauthorized();

        var result = await mediator.Send(new GetServerCategoriesQuery(userId, serverId), ct);
        
        if (!result.IsSuccess)
        {
            return result.Error switch
            {
                ServerError.Forbidden => Forbid(),
                ServerError.NotFound => NotFound(),
                _ => BadRequest()
            };
        }
        
        return Ok(result.Data);
    }
    
    [HttpPost("{serverId:int}/categories")]
    public async Task<IActionResult> CreateCategory(int serverId, [FromBody] CreateCategoryDto dto, CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        if (userId == 0) return Unauthorized();

        var result = await mediator.Send(new CreateCategoryCommand(userId, serverId, dto.Name), ct);

        if (!result.IsSuccess)
        {
            return result.Error switch
            {
                ServerError.Forbidden => Forbid(),
                ServerError.NotFound => NotFound(),
                ServerError.BadRequest => BadRequest(new { message = result.ErrorMessage }),
                _ => StatusCode(500)
            };
        }

        return Ok(result.Data);
    }
}