using System.ComponentModel.DataAnnotations;

namespace BlazorChat.Shared.DTO;

public class CreateCategoryDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}