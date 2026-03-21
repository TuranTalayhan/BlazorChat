using System.Collections.Immutable;
using System.Security.Claims;
using BlazorChat.Server.Data;
using BlazorChat.Server.Data.Entities;
using BlazorChat.Server.Hubs;
using BlazorChat.Shared.DTO;
using BlazorChat.Shared.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BlazorChat.Server.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController(AppDbContext db, IHubContext<ChatHub, IChatClient> hub) : ControllerBase
{
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q = "")
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            return Ok(new List<string>());

        if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var currentId))
            return Unauthorized();

        var users = await db.Users
            .Where(u => u.Id != currentId && u.Username.ToLower().Contains(q.ToLower()))
            .Take(5)
            .Select(u => u.Username)
            .ToListAsync();

        return Ok(users);
    }
    
    [HttpGet("me/status")]
    public async Task<IActionResult> GetStatus()
    {
        if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var currentId))
            return Unauthorized();
        
        var user = await db.Users.FindAsync(currentId);
        if (user == null) return NotFound();
        
        var status = new ReceiveUserStatus
        {
            Id = currentId,
            Status =  user.Status,
        };
        
        return Ok(status);
    }

    [HttpPatch("me/status")]
    public async Task<IActionResult> UpdateStatus([FromBody] UpdateStatusDto dto)
    {
        if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var currentId))
            return Unauthorized();
        
        var user = await db.Users.FindAsync(currentId);
        if (user == null) return NotFound();
        
        var friendIds = await db.Friendships
            .Where(f => (f.RequesterId == currentId || f.ReceiverId == currentId)
                        && f.Status == FriendshipStatus.Accepted)
            .Select(f => f.RequesterId == currentId ? f.ReceiverId.ToString() : f.RequesterId.ToString())
            .ToListAsync();
        
        user.Status = dto.Status;
        var status = new ReceiveUserStatus
        {
            Id = currentId,
            Status =  dto.Status,
        };
        
        await hub.Clients.Users(friendIds).UserStatusChanged(status);
        
        await db.SaveChangesAsync();
        return Ok();
    }
}
