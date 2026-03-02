using System.ComponentModel.DataAnnotations;

namespace BlazorChat.Shared.DTO
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Please enter your email or username.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your password.")]
        public string Password { get; set; } = string.Empty;
        
        public bool RememberMe { get; set; } = false;
    }
}