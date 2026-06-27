namespace BusinessKit.Domain.Entities;

public class ApartmentUnit : BaseEntity
{
    public string? BlockName { get; set; }
    public int? FloorNumber { get; set; }
    public string DoorNumber { get; set; } = string.Empty;
    public string UnitType { get; set; } = string.Empty;
    public decimal? GrossArea { get; set; }
    public decimal? NetArea { get; set; }
    public bool IsOccupied { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
    public ICollection<Resident> Residents { get; set; } = [];
}
