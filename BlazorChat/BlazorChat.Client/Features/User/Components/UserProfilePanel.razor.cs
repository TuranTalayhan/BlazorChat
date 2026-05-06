using BlazorChat.Client.Features.Settings.Dialogs;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BlazorChat.Client.Features.User.Components;

public partial class UserProfilePanel : ComponentBase
{
    protected override async Task OnInitializedAsync()
    {
        await ViewModel.InitializeAsync();
        ViewModel.OnSettingsRequested += HandleOpenSettings;
    }
    
    private void HandleOpenSettings()
    {
        var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true };
        DialogService.ShowAsync<SettingsModal>("Settings", options);
    }

    public void Dispose() => ViewModel.OnSettingsRequested -= HandleOpenSettings;
}