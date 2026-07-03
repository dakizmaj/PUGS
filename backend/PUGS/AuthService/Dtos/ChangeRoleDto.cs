using System.ComponentModel.DataAnnotations;

namespace AuthService.Dtos
{
    public class ChangeRoleDto
    {
        [Required]
        public string Role { get; set; } = string.Empty;
    }
}