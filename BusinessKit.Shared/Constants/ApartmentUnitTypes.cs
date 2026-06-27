namespace BusinessKit.Shared.Constants;

public static class ApartmentUnitTypes
{
    public const string Apartment = "Apartment";
    public const string Office    = "Office";
    public const string Shop      = "Shop";
    public const string Other     = "Other";

    private static readonly HashSet<string> _all = [Apartment, Office, Shop, Other];

    public static bool IsValid(string type) => _all.Contains(type);
}
