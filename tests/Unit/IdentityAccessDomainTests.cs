using TransportPlatform.Modules.IdentityAccess.Domain.Entities;
using TransportPlatform.Modules.IdentityAccess.Domain.Roles;
using Xunit;

namespace TransportPlatform.Unit.Tests;

public sealed class IdentityAccessDomainTests
{
    [Theory]
    [InlineData(IdentityRoles.Passenger)]
    [InlineData(IdentityRoles.Agent)]
    [InlineData(IdentityRoles.AgencyAdmin)]
    [InlineData(IdentityRoles.SuperAdmin)]
    public void UserAcceptsOnlyStableTechnicalRoleCodes(string role)
    {
        var user = CreateUser(role);
        Assert.Equal(role, user.Role);
    }

    [Theory]
    [InlineData("Pasajero")]
    [InlineData("Agente/Vendedor")]
    [InlineData("Administrador de Agencia")]
    [InlineData("Superadministrador")]
    [InlineData("unknown")]
    public void UserRejectsDisplayLabelsAndUnknownRoleCodes(string role)
    {
        Assert.Throws<ArgumentException>(() => CreateUser(role));
    }

    [Fact]
    public void AgencyScopedRolesCanReceiveUniqueCompanyAgencyAssignments()
    {
        var user = CreateUser(IdentityRoles.Agent);
        var companyId = Guid.NewGuid();
        var agencyId = Guid.NewGuid();

        user.AssignAgency(companyId, agencyId);

        var assignment = Assert.Single(user.AgencyAssignments);
        Assert.Equal(user.Id, assignment.UserId);
        Assert.Equal(companyId, assignment.CompanyId);
        Assert.Equal(agencyId, assignment.AgencyId);
        Assert.Throws<InvalidOperationException>(() => user.AssignAgency(companyId, agencyId));
    }

    [Theory]
    [InlineData(IdentityRoles.Passenger)]
    [InlineData(IdentityRoles.SuperAdmin)]
    public void GlobalRolesCannotReceiveAgencyAssignments(string role)
    {
        Assert.Throws<InvalidOperationException>(() => CreateUser(role).AssignAgency(Guid.NewGuid(), Guid.NewGuid()));
    }

    private static User CreateUser(string role) => new(Guid.NewGuid(), "User Example", "12345678",
        "user@example.test", "900000000", "hashed-value", role);
}
