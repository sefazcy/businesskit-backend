using System.ComponentModel.DataAnnotations;

namespace BusinessKit.Application.Customers.Dtos;

public class UpdateCustomerDto
{
    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [EmailAddress]
    [MaxLength(200)]
    public string? Email { get; set; }

    [MaxLength(30)]
    public string? Phone { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }
}
