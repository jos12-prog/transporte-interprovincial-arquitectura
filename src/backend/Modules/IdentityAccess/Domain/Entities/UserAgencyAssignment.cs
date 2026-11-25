namespace TransportPlatform.Modules.IdentityAccess.Domain.Entities;

public sealed class UserAgencyAssignment
{
    private UserAgencyAssignment() { }

    internal UserAgencyAssignment(Guid userId, Guid companyId, Guid agencyId)
    {
        if (userId == Guid.Empty || companyId == Guid.Empty || agencyId == Guid.Empty)
            throw new ArgumentException("User, company and agency ids are required.");
        UserId = userId;
        CompanyId = companyId;
        AgencyId = agencyId;
    }

    public Guid UserId { get; private set; }
    public Guid CompanyId { get; private set; }
    public Guid AgencyId { get; private set; }
}
