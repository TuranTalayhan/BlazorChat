namespace BlazorChat.Shared.Interfaces;

public interface ISidebarItem
{
    int Id { get; }
    
    string DeleteWarningText { get; }
}