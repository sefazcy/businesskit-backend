namespace BusinessKit.Domain.Entities;

public class Appointment : BaseEntity
{
    public string CustomerFullName { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
    public string CustomerPhone { get; set; } = string.Empty;
    public int? StaffMemberId { get; set; }
    public StaffMember? StaffMember { get; set; }
    public int? BusinessServiceId { get; set; }
    public BusinessService? BusinessService { get; set; }
    public DateTime RequestedDate { get; set; }
    public string RequestedTime { get; set; } = string.Empty;
    public string? Note { get; set; }
    public string Status { get; set; } = "Pending";
    public string? AdminNote { get; set; }
}
