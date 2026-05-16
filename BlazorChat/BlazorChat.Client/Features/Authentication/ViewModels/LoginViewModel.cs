using BlazorChat.Shared.DTO;
using Microsoft.AspNetCore.Components;

namespace BlazorChat.Client.Features.Authentication.ViewModels;

public class LoginViewModel(IAuthApiService apiService, NavigationManager nav, ChatAuthStateProvider auth)
{
    public LoginDto Model { get; set; } = new();
    public string ErrorMessage { get; private set; } = "";
    public bool IsLoading { get; set; }

    public async Task CheckUserStatus()
    {
        var state = await auth.GetAuthenticationStateAsync();
        if (state.User.Identity?.IsAuthenticated == true)
            nav.NavigateTo("/chat");
    }

    public async Task LoginUserAsync()
    {
        IsLoading = true;
        ErrorMessage = "";
        
        var meDto = await apiService.LoginAsync(Model);
        
        if (meDto == null)
        {
            ErrorMessage = "Invalid credentials.";
            IsLoading = false;
            return;
        }

        auth.NotifyUserAuthenticated(meDto);
        
        nav.NavigateTo("/chat");

        IsLoading = false;
    }
}