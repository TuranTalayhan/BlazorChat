using System.Security.Claims;
using BlazorChat.Server.Data;
using BlazorChat.Server.Data.Entities;
using BlazorChat.Shared.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlazorChat.Server.Controllers;

[ApiController]
[Route("api/friendships")]
[Authorize]
public class FriendshipsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetFriends()
    {
        if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var currentId))
            return Unauthorized();

        var friends = await db.Friendships
            .Include(f => f.Requester)
            .Include(f => f.Receiver)
            .Where(f => (f.RequesterId == currentId || f.ReceiverId == currentId)
                        && f.Status == FriendshipStatus.Accepted)
            .Select(f => new FriendshipDto
            {
                FriendId = f.RequesterId == currentId ? f.ReceiverId : f.RequesterId,
                Username = f.RequesterId == currentId ? f.Receiver.Username : f.Requester.Username,
                Status = f.RequesterId == currentId ? f.Receiver.Status : f.Requester.Status
            })
            .ToListAsync();

        return Ok(friends);
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingRequests()
    {
        if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var currentId))
            return Unauthorized();

        var pending = await db.Friendships
            .Include(f => f.Requester)
            .Where(f => f.ReceiverId == currentId && f.Status == FriendshipStatus.Pending)
            .Select(f => new PendingFriendshipDto
            {
                RequesterId = f.RequesterId,
                RequesterUsername = f.Requester.Username,
                CreatedAt = f.CreatedAt
            })
            .ToListAsync();

        return Ok(pending);
    }

    [HttpPost]
    public async Task<IActionResult> SendRequest([FromBody] string targetUsername)
    {
        if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var currentId))
            return Unauthorized();

        var target = await db.Users.FirstOrDefaultAsync(u => u.Username == targetUsername);
        if (target == null) return NotFound(new { message = "User not found." });
        if (target.Id == currentId) return BadRequest(new { message = "You cannot add yourself as a friend." });

        var exists = await db.Friendships.AnyAsync(f =>
            (f.RequesterId == currentId && f.ReceiverId == target.Id) ||
            (f.RequesterId == target.Id && f.ReceiverId == currentId));

        if (exists) return Conflict(new { message = "Relationship already exists." });

        db.Friendships.Add(new Friendship
        {
            RequesterId = currentId,
            ReceiverId = target.Id,
            Status = FriendshipStatus.Pending,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
        return Ok();
    }

    [HttpPatch("{requesterId:int}")]
    public async Task<IActionResult> RespondToRequest(int requesterId, [FromBody] bool accept)
    {
        if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var currentId))
            return Unauthorized();

        var friendship = await db.Friendships.FirstOrDefaultAsync(f =>
            f.RequesterId == requesterId && f.ReceiverId == currentId && f.Status == FriendshipStatus.Pending);

        if (friendship == null) return NotFound();

        if (accept)
            friendship.Status = FriendshipStatus.Accepted;
        else
            db.Friendships.Remove(friendship);

        await db.SaveChangesAsync();
        return Ok();
    }
}
