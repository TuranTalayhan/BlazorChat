using System.Security.Claims;
using BlazorChat.Server.Data;
using BlazorChat.Server.Data.Entities;
using BlazorChat.Shared.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlazorChat.Server.Controllers;

[ApiController]
[Route("api/servers")]
[Authorize]
public class ServersController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetMyServers()
    {
        if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
            return Unauthorized();

        var servers = await db.ServerMemberships
            .Where(sm => sm.UserId == userId)
            .Include(sm => sm.Server)
            .Select(sm => new ServerDto
            {
                Id = sm.Server.Id,
                Name = sm.Server.Name,
                OwnerId = sm.Server.OwnerId
            })
            .ToListAsync();

        return Ok(servers);
    }

    [HttpPost]
    public async Task<IActionResult> CreateServer([FromBody] CreateServerDto dto)
    {
        if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
            return Unauthorized();

        var server = new ChatServer
        {
            Name = dto.Name,
            OwnerId = userId,
            CreatedAt = DateTime.UtcNow
        };

        db.Servers.Add(server);
        await db.SaveChangesAsync();

        db.Channels.Add(new Channel { Name = "general", ServerId = server.Id });
        db.ServerMemberships.Add(new ServerMembership { ServerId = server.Id, UserId = userId, Role = ServerRole.Owner });
        await db.SaveChangesAsync();

        return Ok(new ServerDto { Id = server.Id, Name = server.Name, OwnerId = server.OwnerId });
    }

    [HttpGet("{id:int}/channels")]
    public async Task<IActionResult> GetChannels(int id)
    {
        if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
            return Unauthorized();

        var isMember = await db.ServerMemberships.AnyAsync(sm => sm.ServerId == id && sm.UserId == userId);
        if (!isMember) return Forbid();

        var channels = await db.Channels
            .Where(c => c.ServerId == id)
            .OrderBy(c => c.SortOrder).ThenBy(c => c.CreatedAt)
            .Select(c => new ChannelDto { Id = c.Id, Name = c.Name, ServerId = c.ServerId, SortOrder = c.SortOrder })
            .ToListAsync();

        return Ok(channels);
    }
}
