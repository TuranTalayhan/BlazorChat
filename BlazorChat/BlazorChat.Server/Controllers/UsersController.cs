using System.Security.Claims;
using BlazorChat.Server.Data;
using BlazorChat.Shared.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlazorChat.Server.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController(AppDbContext db) : ControllerBase
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

    [HttpPatch("me/status")]
    public async Task<IActionResult> UpdateStatus([FromBody] UpdateStatusDto dto)
    {
        if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var currentId))
            return Unauthorized();

        var user = await db.Users.FindAsync(currentId);
        if (user == null) return NotFound();

        user.Status = dto.Status;
        await db.SaveChangesAsync();
        return Ok();
    }
}
