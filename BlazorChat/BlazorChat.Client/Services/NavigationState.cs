using BlazorChat.Shared.DTO;

namespace BlazorChat.Client.Services;

public class NavigationState
{
    public ServerDto? SelectedServer { get; private set; }
    
    // ADDED: Track the active channel globally
    public int? SelectedChannelId { get; private set; }

    public event Action? OnChanged;

    public void SelectServer(ServerDto? server)
    {
        SelectedServer = server;
        SelectedChannelId = null; 
        OnChanged?.Invoke();
    }

    public void SetActiveChannel(int channelId)
    {
        if (SelectedChannelId == channelId) return;
        SelectedChannelId = channelId;
        OnChanged?.Invoke();
    }
}