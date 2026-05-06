using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BlazorChat.Client.Features.Servers.Dialogs.CreateChannel;

public partial class CreateChannelDialog : ComponentBase
{
    public record ChannelResult(string Name, string? Category);
    [CascadingParameter] IMudDialogInstance MudDialog { get; set; } = null!;
    
    [Parameter] public int ServerId { get; set; }
    [Parameter] public string? DefaultCategory { get; set; }

    private string _channelName = "";
    private string? _category;

    protected override void OnInitialized() => _category = DefaultCategory;

    private void Submit() => MudDialog.Close(DialogResult.Ok(new { Name = _channelName, Category = _category }));
    private void Cancel() => MudDialog.Cancel();
}