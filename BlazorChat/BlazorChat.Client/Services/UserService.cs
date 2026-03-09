using System.Net.Http.Json;
using BlazorChat.Shared.DTO;
using BlazorChat.Shared.Services;

namespace BlazorChat.Client.Services;

public class UserService(HttpClient httpClient) : IUserService
{
    public async Task<List<UserDto>> GetUserAsync(int userId)
    {
        return await httpClient.GetFromJsonAsync<List<UserDto>>($"api/users/{userId}")
               ?? new List<UserDto>();
    }

    public async Task CreateUserAsync(CreateUserDto userDto)
    {
        var response = await httpClient.PostAsJsonAsync($"api/register", userDto);
        response.EnsureSuccessStatusCode();
    }
}