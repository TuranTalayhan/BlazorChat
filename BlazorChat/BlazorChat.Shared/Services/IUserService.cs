using BlazorChat.Shared.DTO;

namespace BlazorChat.Shared.Services;

public interface IUserService
{
    Task<List<UserDto>> GetUserAsync(int userId);
    Task CreateUserAsync(CreateUserDto createUserDto);
}