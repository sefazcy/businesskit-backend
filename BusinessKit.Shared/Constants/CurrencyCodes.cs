namespace BusinessKit.Shared.Constants;

public static class CurrencyCodes
{
    public const string TRY = "TRY";
    public const string USD = "USD";
    public const string EUR = "EUR";
    public const string GBP = "GBP";

    private static readonly HashSet<string> _allowed =
        new(StringComparer.OrdinalIgnoreCase) { TRY, USD, EUR, GBP };

    public const string AllowedList = "TRY, USD, EUR, GBP";

    public static bool IsValid(string? code) =>
        !string.IsNullOrWhiteSpace(code) && _allowed.Contains(code.Trim());
}
