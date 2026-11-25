namespace TransportPlatform.Modules.IdentityAccess.Domain.Roles;

public static class IdentityRoles
{
    public const string Passenger = "passenger";
    public const string Agent = "agent";
    public const string AgencyAdmin = "agency_admin";
    public const string SuperAdmin = "super_admin";

    public static bool IsKnown(string? role) => role is Passenger or Agent or AgencyAdmin or SuperAdmin;

    public static bool HasAgencyScope(string role) => role is Agent or AgencyAdmin;
}
