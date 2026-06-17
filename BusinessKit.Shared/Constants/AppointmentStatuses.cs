namespace BusinessKit.Shared.Constants;

public static class AppointmentStatuses
{
    public const string Pending = "Pending";
    public const string Confirmed = "Confirmed";
    public const string Cancelled = "Cancelled";
    public const string Completed = "Completed";

    private static readonly HashSet<string> _allowed = new() { Pending, Confirmed, Cancelled, Completed };

    public static bool IsValid(string status) => _allowed.Contains(status);

    public static string AllowedValues => string.Join(", ", _allowed);
}
