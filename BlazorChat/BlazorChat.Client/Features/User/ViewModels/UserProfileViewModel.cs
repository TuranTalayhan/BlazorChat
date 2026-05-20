using BlazorChat.Client.Features.Settings.Dialogs;
using BlazorChat.Client.Features.User.Services;
using BlazorChat.Client.Services;
using BlazorChat.Shared.DTO;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using System.Security.Claims;

namespace BlazorChat.Client.Features.User.ViewModels;

public class UserProfileViewModel(
    AuthenticationStateProvider auth, 
    IUserApiService api, 
    IDialogService dialog,
    NavigationState navState)
    : IDisposable
{
    public string Username { get; private set; } = "Guest";
    private int _myUserId;
    private CancellationTokenSource? _cts;
    public UserStatus CurrentStatus { get; private set; } = UserStatus.Online;
    public bool IsStatusPopoverOpen { get; set; }
    public event Action? OnSettingsRequested;
    
    public void RequestSettings() => OnSettingsRequested?.Invoke();

    public void ToggleStatusPopover() => IsStatusPopoverOpen = !IsStatusPopoverOpen;

    public async Task InitializeAsync()
    {
        if(_cts != null)
        {
            await _cts.CancelAsync();
        }
        _cts = new CancellationTokenSource();
        var authState = await auth.GetAuthenticationStateAsync();
        Username = authState.User.Identity?.Name ?? "Guest";
        
        var idClaim = authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(idClaim, out var parsedId))
        {
            _myUserId = parsedId;
        }

        CurrentStatus = await api.GetMyStatusAsync(_cts.Token);
    }

    public async Task UpdateStatusAsync(UserStatus status)
    {
        CurrentStatus = status;
        IsStatusPopoverOpen = false;

        await api.UpdateStatusAsync(status);
        
        if (_myUserId > 0)
        {
            navState.HandleUserStatusChanged(new ReceiveUserStatusDto 
            { 
                Id = _myUserId, 
                Status = status 
            });
        }
    }

    public void OpenSettings()
    {
        var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true };
        dialog.ShowAsync<SettingsModal>("Settings", options);
    }

    public string StatusClass => CurrentStatus switch {
        UserStatus.Online => "online",
        UserStatus.Idle => "idle",
        UserStatus.DoNotDisturb => "dnd",
        _ => "offline"
    };
    
    public string GetClassForStatus(UserStatus status) => status switch {
        UserStatus.Online => "online",
        UserStatus.Idle => "idle",
        UserStatus.DoNotDisturb => "dnd",
        _ => "offline"
    };

    public string StatusLabel => CurrentStatus == UserStatus.DoNotDisturb ? "Do Not Disturb" : CurrentStatus.ToString();

    public void Dispose() => _cts?.Dispose();
}