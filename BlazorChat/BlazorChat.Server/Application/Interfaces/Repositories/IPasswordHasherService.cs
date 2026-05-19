using BlazorChat.Server.Domain.Entities;

namespace BlazorChat.Server.Application.Interfaces.Repositories;

public interface IPasswordHasherService
{
    string HashPassword(User user, string password);
    bool VerifyPassword(User user, string hashedPassword, string providedPassword);
}