namespace BlazorChat.Shared.DTO;

public enum ChannelType
{
    Server = 0,
    DirectMessage = 1,
}

public class ChannelDto
{
    public int Id { get; set; }
    public string? Name { get; set; } = string.Empty;
    public int? ServerId { get; set; }
    public int SortOrder { get; set; }
    public CategoryDto? Category { get; set; }
    public ChannelType Type { get; set; } = ChannelType.Server;
    public List<UserDto> Members { get; set; } = [];
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
