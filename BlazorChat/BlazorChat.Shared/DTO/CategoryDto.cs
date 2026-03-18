namespace BlazorChat.Shared.DTO;

public class CategoryDto
{
    public int Id { get; set; }
    
    public string Name { get; set; } =  string.Empty;
    
    public int ServerId { get; set; }
}