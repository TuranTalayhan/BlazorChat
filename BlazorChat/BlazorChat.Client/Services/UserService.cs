using System.Net.Http.Json;
using BlazorChat.Shared.DTO;
using BlazorChat.Shared.Services;

namespace BlazorChat.Client.Services;

public class UserService(HttpClient httpClient) : IUserService
{
    public async Task<List<UserDto>> GetUserAsync(int userId)
    {
        return await httpClient.GetFromJsonAsync<List<UserDto>>($"api/users/{userId}") 
               ?? [];
    }

    public void CreateUserAsync(CreateUserDto userDto)
    {
        var response = httpClient.PostAsJsonAsync($"api/register", userDto);
        response.Result.EnsureSuccessStatusCode();
    }
}