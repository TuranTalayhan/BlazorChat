using System.Security.Claims;
using BlazorChat.Server.Data;
using BlazorChat.Server.Data.Entities;
using BlazorChat.Shared.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlazorChat.Server.Controllers;

[ApiController]
[Route("api/dms")]
[Authorize]
public class DirectMessagesController(AppDbContext db) : ControllerBase
{
    private int GetUserId() =>
        int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;

    /// <summary>
    /// Get all DM conversations for the current user.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetConversations()
    {
        var userId = GetUserId();
        if (userId == 0) return Unauthorized();

        var conversations = await db.DirectMessages
            .Where(dm => dm.User1Id == userId || dm.User2Id == userId)
            .Include(dm => dm.User1)
            .Include(dm => dm.User2)
            .Select(dm => new DirectMessageDto
            {
                Id = dm.Id,
                OtherUserId = dm.User1Id == userId ? dm.User2Id : dm.User1Id,
                OtherUsername = dm.User1Id == userId ? dm.User2.Username : dm.User1.Username,
                OtherAvatarUrl = dm.User1Id == userId ? dm.User2.AvatarUrl : dm.User1.AvatarUrl,
                OtherStatus = dm.User1Id == userId ? dm.User2.Status : dm.User1.Status
            })
            .ToListAsync();

        return Ok(conversations);
    }

    /// <summary>
    /// Open (or get existing) DM conversation with a user by their ID.
    /// </summary>
    [HttpPost("{targetUserId:int}")]
    public async Task<IActionResult> OpenConversation(int targetUserId)
    {
        var userId = GetUserId();
        if (userId == 0) return Unauthorized();
        if (userId == targetUserId) return BadRequest("Cannot DM yourself.");

        var targetUser = await db.Users.FindAsync(targetUserId);
        if (targetUser == null) return NotFound("User not found.");

        // Canonical ordering: lower id is always User1
        int user1Id = Math.Min(userId, targetUserId);
        int user2Id = Math.Max(userId, targetUserId);

        var existing = await db.DirectMessages
            .FirstOrDefaultAsync(dm => dm.User1Id == user1Id && dm.User2Id == user2Id);

        if (existing != null)
        {
            return Ok(new DirectMessageDto
            {
                Id = existing.Id,
                OtherUserId = targetUserId,
                OtherUsername = targetUser.Username,
                OtherAvatarUrl = targetUser.AvatarUrl,
                OtherStatus = targetUser.Status
            });
        }

        try
        {
            var dm = new DirectMessage { User1Id = user1Id, User2Id = user2Id };
            db.DirectMessages.Add(dm);
            await db.SaveChangesAsync();

            return Ok(new DirectMessageDto
            {
                Id = dm.Id,
                OtherUserId = targetUserId,
                OtherUsername = targetUser.Username,
                OtherAvatarUrl = targetUser.AvatarUrl,
                OtherStatus = targetUser.Status
            });
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            // Concurrent creation — re-query the row that won the race
            var race = await db.DirectMessages
                .FirstOrDefaultAsync(dm => dm.User1Id == user1Id && dm.User2Id == user2Id);
            if (race == null) throw;
            return Ok(new DirectMessageDto
            {
                Id = race.Id,
                OtherUserId = targetUserId,
                OtherUsername = targetUser.Username,
                OtherAvatarUrl = targetUser.AvatarUrl,
                OtherStatus = targetUser.Status
            });
        }
    }
}
