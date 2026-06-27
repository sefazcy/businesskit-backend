namespace BusinessKit.Application.Exceptions;

public class InvalidResidentRoleException : Exception
{
    public InvalidResidentRoleException(string role)
        : base($"'{role}' is not a valid resident role. Allowed values: Owner, Tenant, FamilyMember, Other.") { }
}
