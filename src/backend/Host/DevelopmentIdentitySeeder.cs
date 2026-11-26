using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TransportPlatform.Modules.IdentityAccess.Application.Contracts;
using TransportPlatform.Modules.IdentityAccess.Domain.Entities;
using TransportPlatform.Modules.IdentityAccess.Domain.Roles;
using TransportPlatform.Shared.Contracts.Modules;

namespace TransportPlatform.Host;

internal static class DevelopmentIdentitySeeder
{
    private static readonly string[] SeedCities = ["Ayacucho", "Huanta", "Puquio"];

    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        var configuration = services.GetRequiredService<IConfiguration>();
        var users = services.GetRequiredService<IUserRepository>();
        var hasher = services.GetRequiredService<IPasswordHasher>();
        var companiesAgencies = services.GetRequiredService<ICompaniesAgenciesApi>();
        var companyId = StableId("company:andes-express");
        var agencyIds = SeedCities.Select(city => StableId($"agency:{companyId:N}:{city}")).ToArray();
        var agencies = await companiesAgencies.GetPublicAgenciesAsync(agencyIds, cancellationToken);
        if (agencies.Count != SeedCities.Length)
            throw new InvalidOperationException("Development identity seed could not resolve its agency fixtures.");

        await EnsureUserAsync(users, hasher, configuration, "IDENTITY_SEED_SUPER_ADMIN_PASSWORD",
            "superadmin@rutaandina.test", "Superadministrador", IdentityRoles.SuperAdmin, null, [], cancellationToken);
        await EnsureUserAsync(users, hasher, configuration, "IDENTITY_SEED_AGENCY_ADMIN_PASSWORD",
            "admin.agencia@andesexpress.test", "Administradora Andes Express", IdentityRoles.AgencyAdmin, companyId,
            agencies.Select(agency => agency.AgencyId).ToArray(), cancellationToken);
        await EnsureUserAsync(users, hasher, configuration, "IDENTITY_SEED_AGENT_PASSWORD",
            "agente@andesexpress.test", "Agente Andes Express", IdentityRoles.Agent, companyId,
            agencies.Where(agency => agency.City == "Ayacucho").Select(agency => agency.AgencyId).ToArray(), cancellationToken);
    }

    private static async Task EnsureUserAsync(IUserRepository users, IPasswordHasher hasher,
        IConfiguration configuration, string passwordKey, string email, string fullName, string role, Guid? companyId,
        IReadOnlyCollection<Guid> agencyIds, CancellationToken cancellationToken)
    {
        var existing = await users.FindByIdentifierAsync(email.ToUpperInvariant(), cancellationToken);
        if (existing is not null) return;
        var password = configuration[passwordKey];
        if (string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException($"Development identity password configuration '{passwordKey}' is required.");
        var user = new User(StableId($"identity:{email}"), fullName, null, email.ToUpperInvariant(),
            "000000000", "seed-pending", role);
        var passwordHash = hasher.Hash(user, password);
        user = new User(user.Id, user.FullName, user.NormalizedDni, user.NormalizedEmail,
            user.PhoneNumber, passwordHash, user.Role);
        if (companyId is { } assignmentCompany)
            foreach (var agencyId in agencyIds)
                user.AssignAgency(assignmentCompany, agencyId);
        await users.AddAsync(user, cancellationToken);
    }

    private static Guid StableId(string value)
    {
        var hash = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(value));
        return new Guid(hash.AsSpan(0, 16));
    }
}
