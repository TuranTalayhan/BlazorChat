using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlazorChat.Server.Infrastructure.Persistence;
using BlazorChat.Shared.DTO;
using System.Security.Claims;

namespace BlazorChat.Server.Controllers;

[Authorize]
[ApiController]
[Route("api/servers")]
public class ServerMembersController(AppDbContext db) : ControllerBase
{
    [HttpGet("{serverId:int}/members")]
    public async Task<ActionResult<List<UserDto>>> GetServerMembers(int serverId)
    {
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var isRequestorMember = await db.ServerMemberships
            .AnyAsync(sm => sm.ServerId == serverId && sm.UserId == currentUserId, HttpContext.RequestAborted);

        if (!isRequestorMember) return Forbid();

        var members = await db.ServerMemberships
            .AsNoTracking()
            .Where(sm => sm.ServerId == serverId)
            .OrderByDescending(sm => sm.Role) 
            .ThenBy(sm => sm.User.Username)
            .Select(sm => new UserDto
            {
                Id = sm.User.Id,
                Username = sm.User.Username,
                AvatarUrl = sm.User.AvatarUrl,
                Status = sm.User.Status,
                Role = sm.Role
            })
            .ToListAsync(HttpContext.RequestAborted);

        return Ok(members);
    }
}