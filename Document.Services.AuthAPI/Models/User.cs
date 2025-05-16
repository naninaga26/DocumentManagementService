using System.ComponentModel.DataAnnotations;

namespace Document.Services.AuthAPI.Models;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    public  string Username { get; set; }

    [Required]
    public  string PasswordHash { get; set; } // Store hashed passwords only!

    [Required]
    public Role UserRole { get; set; } = Role.Viewer; // Default role

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}