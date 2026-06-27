namespace BusinessKit.Domain.Entities;

public class Resident : BaseEntity
{
    public int ApartmentUnitId { get; set; }
    public ApartmentUnit ApartmentUnit { get; set; } = null!;
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsPrimary { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public DateTime? MoveInDate { get; set; }
    public DateTime? MoveOutDate { get; set; }
    public string? Notes { get; set; }
}
