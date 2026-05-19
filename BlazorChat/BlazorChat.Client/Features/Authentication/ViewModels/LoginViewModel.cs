using BlazorChat.Shared.DTO;

namespace BlazorChat.Client.Features.Authentication.ViewModels;

public class LoginViewModel(IAuthApiService apiService, ICustomStateUpdater auth)
{
    public LoginDto Model { get; } = new();
    public string ErrorMessage { get; private set; } = "";
    public bool IsLoading { get; private set; }

    public event Action? OnLoginSuccess;

    public async Task CheckUserStatus()
    {
        var state = await auth.GetAuthenticationStateAsync();
        if (state.User.Identity?.IsAuthenticated == true)
        {
            OnLoginSuccess?.Invoke();
        }
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
        
        OnLoginSuccess?.Invoke();

        IsLoading = false;
    }
}