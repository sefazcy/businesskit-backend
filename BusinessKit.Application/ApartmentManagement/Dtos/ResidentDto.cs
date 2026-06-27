namespace BusinessKit.Application.ApartmentManagement.Dtos;

public class ResidentDto
{
    public int Id { get; set; }
    public int ApartmentUnitId { get; set; }
    public string ApartmentDoorNumber { get; set; } = string.Empty;
    public string? ApartmentBlockName { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public bool IsActive { get; set; }
    public DateTime? MoveInDate { get; set; }
    public DateTime? MoveOutDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
