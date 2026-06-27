namespace BusinessKit.Application.ApartmentManagement.Dtos;

public class ResidentListQuery
{
    public string? Search { get; set; }
    public int? ApartmentUnitId { get; set; }
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
    public int Take { get; set; } = 100;
}
