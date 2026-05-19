using BlazorChat.Shared.DTO;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazorChat.Client.Features.Authentication;

public interface ICustomStateUpdater
{
    Task<AuthenticationState> GetAuthenticationStateAsync();
    void NotifyUserAuthenticated(MeDto me);
    void NotifyUserLoggedOut();
}