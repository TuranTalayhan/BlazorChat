using BlazorChat.Client.Features.Friends.Services;
using BlazorChat.Client.Services;
using BlazorChat.Shared.DTO;

namespace BlazorChat.Client.Features.Notifications.ViewModel;

public class NotificationInboxViewModel(IFriendshipApiService apiService)
{
    public List<PendingFriendshipDto> PendingRequests { get; set; } = [];
    
    public async Task InitializeAsync()
    {
        PendingRequests = await apiService.GetPendingRequestsAsync();
    }
    
    public async Task HandleRequestAsync(PendingFriendshipDto request, bool accept)
    {
        var success = await apiService.RespondToRequestAsync(request.RequesterId, accept);
        
        if (success) {
            PendingRequests.Remove(request);
            //snackbar.Add(accept ? "Accepted!" : "Declined", Severity.Success);
        }
    }
}