using System.ComponentModel.DataAnnotations;

namespace AuthService.Infrastructure.Entities
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public string Role { get; set; } = "User";
        public string RefreshToken { get; set; } = string.Empty;
    }
}