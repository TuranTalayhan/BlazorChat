using BlazorChat.Shared.DTO;

namespace BlazorChat.Client.Features.Servers.Dialogs.AddServer;

public class CreateServerViewModel(IServerApiService apiService)
{
    public string? ServerName { get; set; }
    public bool IsSubmitting { get; private set; }
    public string? ErrorMessage { get; private set; }
        
    public event Action? OnChanged;
    
    public string AvatarInitial => !string.IsNullOrWhiteSpace(ServerName) 
        ? ServerName.Substring(0, 1).ToUpper() 
        : "S";

    public bool CanSubmit => !string.IsNullOrWhiteSpace(ServerName) && !IsSubmitting;

    public async Task<ServerDto?> CreateServerAsync()
    {
        if (!CanSubmit) return null;

        IsSubmitting = true;
        ErrorMessage = null;
        NotifyStateChanged();

        try
        {
            var server = await apiService.CreateAsync(new CreateServerDto { Name = ServerName! });
                
            if (server == null)
            {
                ErrorMessage = "The server could not be created. Please try again.";
            }
                
            return server;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"A connection error occurred: {ex.Message}";
            return null;
        }
        finally
        {
            IsSubmitting = false;
            NotifyStateChanged();
        }
    }

    private void NotifyStateChanged() => OnChanged?.Invoke();
}