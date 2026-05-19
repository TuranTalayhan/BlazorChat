using BlazorChat.Server.Application.Features.Auth.Commands;
using BlazorChat.Server.Domain.Entities;
using BlazorChat.Server.Infrastructure.Persistence;
using BlazorChat.Shared.DTO;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BlazorChat.Server.Application.Features.Auth.Handlers;

public class RegisterCommandHandler(AppDbContext db) : ICommandHandler<RegisterCommand, AuthResult>
{
    public async ValueTask<AuthResult> Handle(RegisterCommand request, CancellationToken ct)
    {
        var exists = await db.Users.AnyAsync(u =>
            u.Email.ToLower() == request.Dto.Email.ToLower() ||
            u.Username.ToLower() == request.Dto.Username.ToLower(), ct);

        if (exists)
            return new AuthResult(false, "Username or email already taken.");

        var user = new User 
        { 
            Username = request.Dto.Username, 
            Email = request.Dto.Email, 
            Status = UserStatus.Online 
        };
        
        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, request.Dto.Password);

        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        var meDto = new MeDto { Id = user.Id, Username = user.Username, Email = user.Email, Status = user.Status };
        return new AuthResult(true, null, meDto);
    }
}