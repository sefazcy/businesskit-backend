namespace BusinessKit.Application.ApartmentManagement.Dtos;

public class ApartmentUnitDto
{
    public int Id { get; set; }
    public string? BlockName { get; set; }
    public int? FloorNumber { get; set; }
    public string DoorNumber { get; set; } = string.Empty;
    public string UnitType { get; set; } = string.Empty;
    public decimal? GrossArea { get; set; }
    public decimal? NetArea { get; set; }
    public bool IsOccupied { get; set; }
    public bool IsActive { get; set; }
    public int ResidentCount { get; set; }
    public string? PrimaryResidentName { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
