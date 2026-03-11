using System.Net.Http.Json;
using BlazorChat.Shared.DTO;
using BlazorChat.Shared.Services;

namespace BlazorChat.Client.Services;

public class ServerRepository(HttpClient http) : IServerRepository
{
    public event Action? OnChanged;

    public async Task<List<ServerDto>> GetAllAsync()
    {
        try
        {
            return await http.GetFromJsonAsync<List<ServerDto>>("api/servers") ?? new();
        }
        catch (HttpRequestException)
        {
            return new();
        }
    }

    public async Task<ServerDto?> CreateAsync(CreateServerDto dto)
    {
        var response = await http.PostAsJsonAsync("api/servers", dto);
        if (!response.IsSuccessStatusCode) return null;
        var server = await response.Content.ReadFromJsonAsync<ServerDto>();
        if (server != null) OnChanged?.Invoke();
        return server;
    }
}
