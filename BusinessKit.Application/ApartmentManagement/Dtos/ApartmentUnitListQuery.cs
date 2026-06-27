namespace BusinessKit.Application.ApartmentManagement.Dtos;

public class ApartmentUnitListQuery
{
    public string? Search { get; set; }
    public string? BlockName { get; set; }
    public string? UnitType { get; set; }
    public bool? IsOccupied { get; set; }
    public bool? IsActive { get; set; }
    public int Take { get; set; } = 100;
}
