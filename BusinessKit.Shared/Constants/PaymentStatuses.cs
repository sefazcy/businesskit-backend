namespace BusinessKit.Shared.Constants;

public static class PaymentStatuses
{
    public const string Pending   = "Pending";
    public const string Paid      = "Paid";
    public const string Failed    = "Failed";
    public const string Cancelled = "Cancelled";
    public const string Refunded  = "Refunded";

    private static readonly HashSet<string> _all = [Pending, Paid, Failed, Cancelled, Refunded];

    public static bool IsValid(string status) => _all.Contains(status);
}
