using TransportPlatform.Modules.IdentityAccess.Domain.Roles;

namespace TransportPlatform.Modules.IdentityAccess.Domain.Entities;

public sealed class User
{
    private readonly List<UserAgencyAssignment> _agencyAssignments = [];

    private User() { }

    public User(Guid id, string fullName, string? normalizedDni, string normalizedEmail,
        string phoneNumber, string passwordHash, string role)
    {
        if (id == Guid.Empty) throw new ArgumentException("User id is required.", nameof(id));
        if (string.IsNullOrWhiteSpace(fullName)) throw new ArgumentException("Name is required.", nameof(fullName));
        if (string.IsNullOrWhiteSpace(normalizedEmail)) throw new ArgumentException("Email is required.", nameof(normalizedEmail));
        if (string.IsNullOrWhiteSpace(phoneNumber)) throw new ArgumentException("Phone number is required.", nameof(phoneNumber));
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("Password hash is required.", nameof(passwordHash));
        if (!IdentityRoles.IsKnown(role)) throw new ArgumentException("Unknown role code.", nameof(role));
        if (role == IdentityRoles.Passenger && string.IsNullOrWhiteSpace(normalizedDni))
            throw new ArgumentException("Passenger DNI is required.", nameof(normalizedDni));

        Id = id;
        FullName = fullName.Trim();
        NormalizedDni = string.IsNullOrWhiteSpace(normalizedDni) ? null : normalizedDni.Trim();
        NormalizedEmail = normalizedEmail.Trim();
        PhoneNumber = phoneNumber.Trim();
        PasswordHash = passwordHash;
        Role = role;
    }

    public Guid Id { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public string? NormalizedDni { get; private set; }
    public string NormalizedEmail { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string Role { get; private set; } = string.Empty;
    public IReadOnlyCollection<UserAgencyAssignment> AgencyAssignments => _agencyAssignments.AsReadOnly();

    public void AssignAgency(Guid companyId, Guid agencyId)
    {
        if (!IdentityRoles.HasAgencyScope(Role))
            throw new InvalidOperationException("Only agency-scoped staff roles can be assigned to an agency.");
        if (companyId == Guid.Empty) throw new ArgumentException("Company id is required.", nameof(companyId));
        if (agencyId == Guid.Empty) throw new ArgumentException("Agency id is required.", nameof(agencyId));
        if (_agencyAssignments.Any(a => a.AgencyId == agencyId))
            throw new InvalidOperationException("The agency is already assigned to this user.");

        _agencyAssignments.Add(new UserAgencyAssignment(Id, companyId, agencyId));
    }
}
