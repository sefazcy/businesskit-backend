using System.ComponentModel.DataAnnotations;

namespace BusinessKit.Application.ApartmentManagement.Dtos;

public class CreateResidentRequest
{
    [Required(ErrorMessage = "ApartmentUnitId is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "ApartmentUnitId must be a positive integer.")]
    public int ApartmentUnitId { get; set; }

    [Required(ErrorMessage = "FullName is required.")]
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? Phone { get; set; }

    [EmailAddress]
    [MaxLength(200)]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Role is required.")]
    [MaxLength(30)]
    public string Role { get; set; } = string.Empty;

    public bool IsPrimary { get; set; } = false;

    public bool IsActive { get; set; } = true;

    public DateTime? MoveInDate { get; set; }

    public DateTime? MoveOutDate { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }
}
