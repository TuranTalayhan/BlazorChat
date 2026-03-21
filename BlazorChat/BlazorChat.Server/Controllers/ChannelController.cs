using System.Security.Claims;
using BlazorChat.Server.Data;
using BlazorChat.Server.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlazorChat.Server.Controllers;

[ApiController]
[Route("api/channels")]
[Authorize]
public class ChannelsController(AppDbContext db) : ControllerBase
{
    private int GetUserId() =>
        int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;

    [HttpPost("dm/{friendId}")]
    public async Task<IActionResult> GetOrCreateDm(int friendId)
    {
        var currentUserId = GetUserId();
        if (currentUserId == 0) return Unauthorized();

        if (currentUserId == friendId) 
            return BadRequest("Cannot create a DM with yourself.");

        var existingDmId = await db.Channels
            .Where(c => c.Type == ChannelType.DirectMessage)
            .Where(c => c.Members.Any(m => m.Id == currentUserId) && 
                        c.Members.Any(m => m.Id == friendId))
            .Select(c => c.Id)
            .FirstOrDefaultAsync();
        
        if (existingDmId > 0)
        {
            return Ok(existingDmId);
        }

        var currentUser = await db.Users.FindAsync(currentUserId);
        var friendUser = await db.Users.FindAsync(friendId);

        if (currentUser == null || friendUser == null)
            return NotFound("User not found.");

        var newDmChannel = new Channel
        {
            Type = ChannelType.DirectMessage,
            Members = new List<User> { currentUser, friendUser }
        };

        db.Channels.Add(newDmChannel);
        await db.SaveChangesAsync();

        return Ok(newDmChannel.Id);
    }
}