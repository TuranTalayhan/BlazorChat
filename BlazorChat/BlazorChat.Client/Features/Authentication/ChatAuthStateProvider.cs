using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using BlazorChat.Client.Core;
using BlazorChat.Shared.DTO;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazorChat.Client.Features.Authentication;

public class ChatAuthStateProvider(HttpClient http) : AuthenticationStateProvider, ICustomStateUpdater
{
    private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var statusResponse = await http.GetAsync(ApiRoutes.Auth.Status);
            
            if (statusResponse.StatusCode == HttpStatusCode.Unauthorized || !statusResponse.IsSuccessStatusCode)
            {
                return new AuthenticationState(_anonymous);
            }

            var status = await statusResponse.Content.ReadFromJsonAsync<StatusDto>();
            if (status is not { IsAuthenticated: true }) 
            {
                return new AuthenticationState(_anonymous);
            }
            
            var meResponse = await http.GetAsync(ApiRoutes.Auth.Me);
            if (!meResponse.IsSuccessStatusCode)
            {
                return new AuthenticationState(_anonymous);
            }

            var me = await meResponse.Content.ReadFromJsonAsync<MeDto>();
            if (me == null) 
            {
                return new AuthenticationState(_anonymous);
            }

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
        catch (Exception ex)
        {
            Console.WriteLine($"Authentication check failed cleanly: {ex.Message}");
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