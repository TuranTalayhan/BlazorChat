using System.ComponentModel.DataAnnotations;

namespace BlazorChat.Shared.DTO;

public class CreateServerChannelDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } =  string.Empty;
    
    public string? CategoryName { get; set; } 
    public int? CategoryId { get; set; }
}