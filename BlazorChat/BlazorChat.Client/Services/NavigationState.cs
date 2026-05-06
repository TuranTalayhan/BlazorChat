using BlazorChat.Shared.DTO;

namespace BlazorChat.Client.Services;

public class NavigationState
{
    public ServerDto? SelectedServer { get; private set; }
    public event Action? OnChanged;

    public void SelectServer(ServerDto? server)
    {
        SelectedServer = server;
        OnChanged?.Invoke();
    }
}