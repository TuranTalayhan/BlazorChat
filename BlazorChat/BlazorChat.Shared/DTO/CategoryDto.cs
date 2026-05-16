using BlazorChat.Shared.Interfaces;

namespace BlazorChat.Shared.DTO;

public class CategoryDto : ISidebarItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    
    public int ServerId { get; set; }
    
    public string DeleteWarningText => $"the category {Name}";
}