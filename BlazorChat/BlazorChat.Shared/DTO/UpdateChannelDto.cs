namespace BlazorChat.Shared.DTO;

public class UpdateChannelDto
{
    public string? Name { get; set; }
    public int? CategoryId { get; set; }
    public int? SortOrder { get; set; }
}