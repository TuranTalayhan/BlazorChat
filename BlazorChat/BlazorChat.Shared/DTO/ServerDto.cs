namespace BlazorChat.Shared.DTO;

public class ServerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? IconUrl { get; set; }
    public int OwnerId { get; set; }
}
