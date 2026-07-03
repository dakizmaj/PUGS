using System;

namespace AuthService.Dtos
{
    public class AdminUserDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsLdapUser { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}