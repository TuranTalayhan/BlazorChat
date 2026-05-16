using System.Net.Http.Json;
using BlazorChat.Shared.DTO;

namespace BlazorChat.Client.Features.Servers.Services;

public interface IServerApiService
{
    public event Action? OnChanged;
    Task<List<ServerDto>> GetAllMyServersAsync();
    
    Task<List<ServerDto>> GetAllServersAsync(int id);

    Task<ServerDto?> CreateAsync(CreateServerDto dto);
}

public class ServerApiService(HttpClient http) : IServerApiService
{
    public event Action? OnChanged;

    public async Task<List<ServerDto>> GetAllMyServersAsync()
    {
        try
        {
            return await http.GetFromJsonAsync<List<ServerDto>>("api/servers/@me") ?? [];
        }
        catch (HttpRequestException)
        {
            return [];
        }
    }

    public async Task<List<ServerDto>> GetAllServersAsync(int id)
    {
        return await http.GetFromJsonAsync<List<ServerDto>>("api/servers") ?? [];
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
