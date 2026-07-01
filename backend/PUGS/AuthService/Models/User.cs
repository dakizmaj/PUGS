using System;
using System.ComponentModel.DataAnnotations;

namespace AuthService.Models
{
    public enum UserRole
    {
        User,
        Admin
    }

    public class User
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        // Hesirana lozinka - nikad plain text
        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public UserRole Role { get; set; } = UserRole.User;

        // Opciono - za LDAP SSO korisnike, lozinka moze biti null/empty
        public bool IsLdapUser { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}