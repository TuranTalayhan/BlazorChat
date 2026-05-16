using System.Net.Http.Json;
using BlazorChat.Client.Core;
using BlazorChat.Shared.DTO;

namespace BlazorChat.Client.Features.User.Services;

public interface IUserApiService
{
    Task<UserStatus> GetMyStatusAsync(CancellationToken ct = default);
    Task UpdateStatusAsync(UserStatus status);
}

public class UserApiService(HttpClient http) : IUserApiService
{
    public async Task<UserStatus> GetMyStatusAsync(CancellationToken ct)
    {
        var result = await http.GetFromJsonAsync<ReceiveUserStatusDto>(ApiRoutes.Users.GetStatus, ct);
        return result?.Status ?? UserStatus.Offline;
    }

    public async Task UpdateStatusAsync(UserStatus status)
    {
        await http.PatchAsJsonAsync(ApiRoutes.Users.UpdateStatus, new UpdateStatusDto { Status = status });
    }
}