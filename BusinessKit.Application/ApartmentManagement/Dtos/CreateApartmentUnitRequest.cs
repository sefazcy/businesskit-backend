using System.ComponentModel.DataAnnotations;

namespace BusinessKit.Application.ApartmentManagement.Dtos;

public class CreateApartmentUnitRequest
{
    [MaxLength(50)]
    public string? BlockName { get; set; }

    public int? FloorNumber { get; set; }

    [Required(ErrorMessage = "DoorNumber is required.")]
    [MaxLength(30)]
    public string DoorNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "UnitType is required.")]
    [MaxLength(30)]
    public string UnitType { get; set; } = string.Empty;

    [Range(0, double.MaxValue, ErrorMessage = "GrossArea cannot be negative.")]
    public decimal? GrossArea { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "NetArea cannot be negative.")]
    public decimal? NetArea { get; set; }

    public bool IsOccupied { get; set; } = false;

    public bool IsActive { get; set; } = true;

    [MaxLength(1000)]
    public string? Notes { get; set; }
}
