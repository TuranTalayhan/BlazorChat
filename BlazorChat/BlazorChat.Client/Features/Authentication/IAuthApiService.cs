using System.Net.Http.Json;
using BlazorChat.Client.Core;
using BlazorChat.Shared.DTO;

namespace BlazorChat.Client.Features.Authentication;

public interface IAuthApiService
{
    Task<MeDto?> LoginAsync(LoginDto credentials);
}

public class AuthApiService(HttpClient http) : IAuthApiService
{
    public async Task<MeDto?> LoginAsync(LoginDto credentials)
    {
        var response = await http.PostAsJsonAsync(ApiRoutes.Auth.Login, credentials);
        return response.IsSuccessStatusCode 
            ? await response.Content.ReadFromJsonAsync<MeDto>() 
            : null;
    }
}