using BlazorChat.Server.Application.Features.Auth.Commands;
using BlazorChat.Server.Domain.Entities;
using BlazorChat.Server.Infrastructure.Persistence;
using BlazorChat.Shared.DTO;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BlazorChat.Server.Application.Features.Auth.Handlers;

public class LoginCommandHandler(AppDbContext db) : ICommandHandler<LoginCommand, AuthResult>
{
    public async ValueTask<AuthResult> Handle(LoginCommand request, CancellationToken ct)
    {
        var identifier = request.Dto.Email.ToLower().Trim();
        var user = await db.Users.FirstOrDefaultAsync(u =>
            u.Email.ToLower() == identifier || u.Username.ToLower() == identifier, ct);

        if (user == null)
            return new AuthResult(false, "Invalid credentials.");

        var hasher = new PasswordHasher<User>();
        var result = hasher.VerifyHashedPassword(user, user.PasswordHash, request.Dto.Password);
        
        if (result == PasswordVerificationResult.Failed)
            return new AuthResult(false, "Invalid credentials.");

        var meDto = new MeDto { Id = user.Id, Username = user.Username, Email = user.Email, Status = user.Status };
        return new AuthResult(true, null, meDto);
    }
}