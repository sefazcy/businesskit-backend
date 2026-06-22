namespace BusinessKit.Shared.Constants;

public static class StockMovementTypes
{
    public const string In         = "In";
    public const string Out        = "Out";
    public const string Adjustment = "Adjustment";

    private static readonly HashSet<string> _all = [In, Out, Adjustment];

    public static bool IsValid(string type) => _all.Contains(type);
}
