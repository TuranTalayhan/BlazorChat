using System.Security.Claims;
using Mediator;
using BlazorChat.Server.Application.Features.Auth.Commands;
using BlazorChat.Shared.DTO;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlazorChat.Server.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IMediator mediator) : ControllerBase
{
    [HttpGet("status")]
    [AllowAnonymous]
    public IActionResult GetAuthStatus()
    {
        return Ok(new { IsAuthenticated = User.Identity?.IsAuthenticated ?? false });
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken ct)
    {
        var result = await mediator.Send(new LoginCommand(dto), ct);

        if (!result.IsSuccess)
            return Unauthorized(new { message = result.ErrorMessage });

        await SignInUser(result.User!);
        return Ok(result.User);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] CreateUserDto dto, CancellationToken ct)
    {
        var result = await mediator.Send(new RegisterCommand(dto), ct);

        if (!result.IsSuccess)
            return Conflict(new { message = result.ErrorMessage });

        await SignInUser(result.User!);
        return Ok(result.User);
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
        var idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var statusStr = User.FindFirst("status")?.Value;
        
        return Ok(new MeDto
        {
            Id = int.TryParse(idStr, out var uid) ? uid : 0,
            Username = User.Identity?.Name ?? "",
            Email = User.FindFirst(ClaimTypes.Email)?.Value ?? "",
            Status = Enum.TryParse<UserStatus>(statusStr, out var s) ? s : UserStatus.Online
        });
    }
    
    private async Task SignInUser(MeDto user)
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