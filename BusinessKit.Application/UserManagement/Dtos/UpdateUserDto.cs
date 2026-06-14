using System.ComponentModel.DataAnnotations;

namespace BusinessKit.Application.UserManagement.Dtos;

public class UpdateUserDto
{
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public bool IsActive { get; set; }

    [Required]
    public List<string> Roles { get; set; } = new();
}
