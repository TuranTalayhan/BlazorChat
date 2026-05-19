using BlazorChat.Server.Application.Features.Auth.Commands;
using BlazorChat.Server.Application.Interfaces.Repositories;
using BlazorChat.Shared.DTO;
using Mediator;

namespace BlazorChat.Server.Application.Features.Auth.Handlers;

public class LoginCommandHandler(IUserRepository userRepository, IPasswordHasherService passwordHasher) 
    : ICommandHandler<LoginCommand, AuthResult>
{
    public async ValueTask<AuthResult> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await userRepository.GetByIdentifierAsync(request.Dto.Email, ct);
        if (user == null)
        {
            return new AuthResult(false, "Invalid credentials.");
        }

        var isPasswordValid = passwordHasher.VerifyPassword(user, user.PasswordHash, request.Dto.Password);
        if (!isPasswordValid)
        {
            return new AuthResult(false, "Invalid credentials.");
        }

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