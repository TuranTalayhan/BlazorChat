using System.Net.Http.Json;
using System.Security.Claims;
using BlazorChat.Client.Core;
using BlazorChat.Shared.DTO;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazorChat.Client.Features.Authentication;

public class ChatAuthStateProvider(HttpClient http) : AuthenticationStateProvider
{
    private ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var status = await http.GetFromJsonAsync<StatusDto>(ApiRoutes.Auth.Status);
            
            if (status is { IsAuthenticated: false }) return new AuthenticationState(_anonymous);
            
            var me = await http.GetFromJsonAsync<MeDto>(ApiRoutes.Auth.Me);
            if (me == null) return new AuthenticationState(_anonymous);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, me.Username),
                new Claim(ClaimTypes.NameIdentifier, me.Id.ToString()),
                new Claim(ClaimTypes.Email, me.Email),
                new Claim("status", me.Status.ToString())
            };
            var identity = new ClaimsIdentity(claims, "cookieAuth");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        catch
        {
            return new AuthenticationState(_anonymous);
        }
    }

    public void NotifyUserAuthenticated(MeDto me)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, me.Username),
            new Claim(ClaimTypes.NameIdentifier, me.Id.ToString()),
            new Claim(ClaimTypes.Email, me.Email),
            new Claim("status", me.Status.ToString())
        };
        var identity = new ClaimsIdentity(claims, "cookieAuth");
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity))));
    }

    public void NotifyUserLoggedOut()
    {
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
    }
}
