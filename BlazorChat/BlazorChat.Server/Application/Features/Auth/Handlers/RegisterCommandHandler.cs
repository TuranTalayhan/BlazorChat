using BlazorChat.Server.Application.Features.Auth.Commands;
using BlazorChat.Server.Application.Interfaces.Repositories;
using BlazorChat.Server.Domain.Entities;
using BlazorChat.Shared.DTO;
using Mediator;

namespace BlazorChat.Server.Application.Features.Auth.Handlers;

public class RegisterCommandHandler(IUserRepository userRepository, IPasswordHasherService passwordHasher) 
    : ICommandHandler<RegisterCommand, AuthResult>
{
    public async ValueTask<AuthResult> Handle(RegisterCommand request, CancellationToken ct)
    {
        var exists = await userRepository.ExistsAsync(request.Dto.Email, request.Dto.Username, ct);
        if (exists)
        {
            return new AuthResult(false, "Username or email already taken.");
        }

        var user = User.Create(
            request.Dto.Username, 
            request.Dto.Email, 
            request.Dto.Password, 
            passwordHasher.HashPassword
        );

        await userRepository.AddAsync(user, ct);
        await userRepository.SaveChangesAsync(ct);

        var meDto = new MeDto 
        { 
            Id = user.Id, 
            Username = user.Username, 
            Email = user.Email, 
            Status = user.Status 
        };
        
        return new AuthResult(true, null, meDto);
    }
}