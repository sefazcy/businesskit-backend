namespace BusinessKit.Shared.Constants;

public static class ResidentRoles
{
    public const string Owner        = "Owner";
    public const string Tenant       = "Tenant";
    public const string FamilyMember = "FamilyMember";
    public const string Other        = "Other";

    private static readonly HashSet<string> _all = [Owner, Tenant, FamilyMember, Other];

    public static bool IsValid(string role) => _all.Contains(role);
}
