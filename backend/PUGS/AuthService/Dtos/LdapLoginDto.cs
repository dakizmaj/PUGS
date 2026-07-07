using System.ComponentModel.DataAnnotations;

namespace AuthService.Dtos
{
    public class LdapLoginDto
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}