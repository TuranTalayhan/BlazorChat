using System.Text.RegularExpressions;
using BlazorChat.Client.Features.Channels;
using BlazorChat.Client.Features.Servers.Dialogs.CreateChannel;
using BlazorChat.Shared.DTO;
using Mediator;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using MudBlazor;

namespace BlazorChat.Client.Features.Servers.ViewModels;

public class SidebarViewModel(IMediator mediator, NavigationManager nav, IDialogService dialog)
{
    public List<ChannelDto> Channels { get; private set; } = [];
    public bool IsLoading { get; private set; }
    public int ActiveChannelId { get; private set; }
    public HashSet<string> CollapsedCategories { get; } = new();

    public void Initialize() => nav.LocationChanged += OnLocationChanged;
    public void Dispose() => nav.LocationChanged -= OnLocationChanged;

    public async Task LoadChannelsAsync(int serverId)
    {
        IsLoading = true;
        Channels = await mediator.Send(new GetChannelsQuery(serverId));
        UpdateActiveChannel();
        IsLoading = false;
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e) => UpdateActiveChannel();

    public void UpdateActiveChannel()
    {
        var match = Regex.Match(nav.Uri, @"/chat/(\d+)");
        ActiveChannelId = match.Success ? int.Parse(match.Groups[1].Value) : 0;
    }

    public void ToggleCategory(string name)
    {
        if (!CollapsedCategories.Add(name)) CollapsedCategories.Remove(name);
    }

    public IEnumerable<ChannelDto> RootChannels => 
        Channels.Where(c => c.Category == null).OrderBy(c => c.SortOrder);

    public IEnumerable<IGrouping<string, ChannelDto>> CategorizedGroups =>
        Channels.Where(c => c.Category != null)
            .GroupBy(c => c.Category!.Name)
            .OrderBy(g => g.First().Category!.SortOrder);

    public async Task OpenCreateChannelDialog(int serverId, string? category = null)
    {
        var parameters = new DialogParameters<CreateChannelDialog> { { x => x.ServerId, serverId } };
        var result = await dialog.ShowAsync<CreateChannelDialog>("New Channel", parameters);
        if (!(await result.Result).Canceled) await LoadChannelsAsync(serverId);
    }

    public void NavTo(int id) => nav.NavigateTo($"/chat/{id}");
}