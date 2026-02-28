using System.ComponentModel.DataAnnotations;

namespace BlazorChat.Shared.DTO
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Email is required to log in.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your password.")]
        public string Password { get; set; } = string.Empty;
        
        public bool RememberMe { get; set; } = false;
    }
}