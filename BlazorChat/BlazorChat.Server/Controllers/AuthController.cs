using System.Security.Claims;
using BlazorChat.Server.Data;
using BlazorChat.Server.Data.Entities;
using BlazorChat.Shared.DTO;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlazorChat.Server.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AppDbContext db) : ControllerBase
{
    [HttpGet("status")]
    [AllowAnonymous]
    public IActionResult GetAuthStatus()
    {
        return Ok(new { IsAuthenticated = User.Identity?.IsAuthenticated ?? false });
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var identifier = dto.Email.ToLower().Trim();
        var user = await db.Users.FirstOrDefaultAsync(u =>
            u.Email.ToLower() == identifier || u.Username.ToLower() == identifier);

        if (user == null)
            return Unauthorized(new { message = "Invalid credentials." });

        var hasher = new PasswordHasher<User>();
        var result = hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
        if (result == PasswordVerificationResult.Failed)
            return Unauthorized(new { message = "Invalid credentials." });

        await SignInUser(user);
        return Ok(new MeDto { Id = user.Id, Username = user.Username, Email = user.Email, Status = user.Status });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] CreateUserDto dto)
    {
        if (await db.Users.AnyAsync(u =>
                u.Email.ToLower() == dto.Email.ToLower() ||
                u.Username.ToLower() == dto.Username.ToLower()))
            return Conflict(new { message = "Username or email already taken." });

        var user = new User { Username = dto.Username, Email = dto.Email, Status = UserStatus.Online };
        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, dto.Password);

        db.Users.Add(user);
        await db.SaveChangesAsync();

        await SignInUser(user);
        return Ok(new MeDto { Id = user.Id, Username = user.Username, Email = user.Email, Status = user.Status });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok();
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var name = User.Identity?.Name;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var statusStr = User.FindFirst("status")?.Value;
        var status = Enum.TryParse<UserStatus>(statusStr, out var s) ? s : UserStatus.Online;

        return Ok(new MeDto
        {
            Id = int.TryParse(id, out var uid) ? uid : 0,
            Username = name ?? "",
            Email = email ?? "",
            Status = status
        });
    }

    private async Task SignInUser(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new("status", user.Status.ToString())
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
    }
}
