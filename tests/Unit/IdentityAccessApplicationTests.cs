using TransportPlatform.Modules.IdentityAccess.Application.Authorization;
using TransportPlatform.Modules.IdentityAccess.Application.Contracts;
using TransportPlatform.Modules.IdentityAccess.Application.Models;
using TransportPlatform.Modules.IdentityAccess.Application.UseCases;
using TransportPlatform.Modules.IdentityAccess.Domain.Entities;
using TransportPlatform.Modules.IdentityAccess.Domain.Roles;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using System.Text.Json;
using TransportPlatform.Modules.IdentityAccess.Infrastructure.Security;
using Xunit;

namespace TransportPlatform.Unit.Tests;

public sealed class IdentityAccessApplicationTests
{
    [Fact]
    public async Task RegistrationReturnsConflictForDuplicateEmailOrDni()
    {
        var repository = new FakeUsers { Exists = true };
        var result = await new RegisterPassengerUseCase(repository, new FakeHasher())
            .ExecuteAsync(new RegisterPassengerCommand("Ana User", "12345678", "ana@example.test", "900000000", "secret123"));

        Assert.Equal(IdentityFailure.Conflict, result.Failure);
        Assert.Empty(repository.Added);
    }

    [Fact]
    public async Task LoginNormalizesEmailAndDniIdentifiersBeforeLookingUpUser()
    {
        var user = CreateUser(IdentityRoles.Passenger);
        var repository = new FakeUsers { Found = user };
        var attemptStore = new FakeAttempts();
        var useCase = new LoginUseCase(repository, new FakeHasher(), new FakeTokenIssuer(), attemptStore, new FakeClock());

        var emailResult = await useCase.ExecuteAsync(new LoginCommand(" USER@EXAMPLE.TEST ", "secret123"));
        var dniResult = await useCase.ExecuteAsync(new LoginCommand(" 12345678 ", "secret123"));

        Assert.True(emailResult.Succeeded);
        Assert.True(dniResult.Succeeded);
        Assert.Equal(new[] { "USER@EXAMPLE.TEST", "12345678" }, repository.Lookups);
        Assert.Equal(IdentityRoles.Passenger, emailResult.Value!.User.Role);
    }

    [Fact]
    public void AgencyScopeAllowsOnlyTheAssignedCompanyAndAgencyPair()
    {
        var user = CreateUser(IdentityRoles.AgencyAdmin);
        var company = Guid.NewGuid();
        var agency = Guid.NewGuid();
        user.AssignAgency(company, agency);
        var scope = new AgencyScope();

        Assert.True(scope.Allows(user, company, agency));
        Assert.False(scope.Allows(user, company, Guid.NewGuid()));
        Assert.False(scope.Allows(user, Guid.NewGuid(), agency));
        Assert.False(scope.Allows(CreateUser(IdentityRoles.Passenger), company, agency));
    }

    [Fact]
    public async Task LoginAllowsFiveFailuresThenReturnsRateLimitedOnTheNextAttempt()
    {
        var attempts = new FakeAttempts();
        var useCase = new LoginUseCase(new FakeUsers(), new FakeHasher(), new FakeTokenIssuer(), attempts, new FakeClock());
        var command = new LoginCommand("unknown@example.test", "wrong-password");

        for (var failure = 0; failure < 5; failure++)
            Assert.Equal(IdentityFailure.InvalidCredentials, (await useCase.ExecuteAsync(command)).Failure);

        Assert.Equal(IdentityFailure.RateLimited, (await useCase.ExecuteAsync(command)).Failure);
        Assert.Equal(5, attempts.FailureCount);
    }

    [Fact]
    public void PasswordHasherStoresAHashAndVerifiesOnlyTheOriginalPassword()
    {
        var user = CreateUser(IdentityRoles.Passenger);
        var hasher = new AspNetPasswordHasher(new PasswordHasher<User>());
        var passwordHash = hasher.Hash(user, "my-strong-password");
        var savedUser = new User(user.Id, user.FullName, user.NormalizedDni, user.NormalizedEmail,
            user.PhoneNumber, passwordHash, user.Role);

        Assert.NotEqual("my-strong-password", passwordHash);
        Assert.True(hasher.Verify(savedUser, "my-strong-password"));
        Assert.False(hasher.Verify(savedUser, "wrong-password"));
    }

    [Fact]
    public void JwtContainsStableRoleAndTenantClaimsAndExpiresAfterSixtyMinutes()
    {
        using var key = RSA.Create(2048);
        using var tokens = new JwtAccessTokenService("transport-platform", key.ExportRSAPrivateKeyPem(), key.ExportRSAPublicKeyPem());
        var user = CreateUser(IdentityRoles.Agent);
        var companyId = Guid.NewGuid();
        var agencyId = Guid.NewGuid();
        user.AssignAgency(companyId, agencyId);
        var issuedAt = DateTimeOffset.UtcNow;
        var token = tokens.Issue(user, issuedAt);
        var payload = JsonDocument.Parse(FromBase64Url(token.Split('.')[1])).RootElement;

        Assert.Equal(IdentityRoles.Agent, payload.GetProperty("role").GetString());
        Assert.Equal(user.Id.ToString(), payload.GetProperty("sub").GetString());
        Assert.Equal(companyId.ToString(), payload.GetProperty("company_id").GetString());
        Assert.Equal(agencyId.ToString(), payload.GetProperty("agency_ids")[0].GetString());
        Assert.Equal(issuedAt.AddMinutes(60).ToUnixTimeSeconds(), payload.GetProperty("exp").GetInt64());
        Assert.NotNull(tokens.Validate(token));
    }

    [Fact]
    public void JwtValidatorRejectsExpiredTokens()
    {
        using var key = RSA.Create(2048);
        using var tokens = new JwtAccessTokenService("transport-platform", key.ExportRSAPrivateKeyPem(), key.ExportRSAPublicKeyPem());
        var expired = tokens.Issue(CreateUser(IdentityRoles.Passenger), DateTimeOffset.UtcNow.AddMinutes(-61));

        Assert.Null(tokens.Validate(expired));
    }

    [Fact]
    public void JwtIssuerRejectsMissingConfiguration()
    {
        Assert.Throws<InvalidOperationException>(() => new JwtAccessTokenService("", "private", "public"));
    }

    private static byte[] FromBase64Url(string value)
    {
        var normalized = value.Replace('-', '+').Replace('_', '/');
        normalized += (normalized.Length % 4) switch { 2 => "==", 3 => "=", _ => string.Empty };
        return Convert.FromBase64String(normalized);
    }

    private static User CreateUser(string role) => new(Guid.NewGuid(), "User Example", "12345678",
        "USER@EXAMPLE.TEST", "900000000", "hashed:secret123", role);

    private sealed class FakeUsers : IUserRepository
    {
        public bool Exists { get; init; }
        public User? Found { get; init; }
        public List<string> Lookups { get; } = [];
        public List<User> Added { get; } = [];

        public Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(Found?.Id == id ? Found : null);

        public Task<User?> FindByIdentifierAsync(string normalizedIdentifier, CancellationToken cancellationToken)
        {
            Lookups.Add(normalizedIdentifier);
            return Task.FromResult(Found);
        }

        public Task<bool> ExistsByEmailOrDniAsync(string normalizedEmail, string normalizedDni, CancellationToken cancellationToken) => Task.FromResult(Exists);
        public Task AddAsync(User user, CancellationToken cancellationToken) { Added.Add(user); return Task.CompletedTask; }
    }

    private sealed class FakeHasher : IPasswordHasher
    {
        public string Hash(User user, string password) => $"hashed:{password}";
        public bool Verify(User user, string password) => user.PasswordHash == $"hashed:{password}";
    }

    private sealed class FakeTokenIssuer : IAccessTokenIssuer
    {
        public string Issue(User user, DateTimeOffset issuedAtUtc) => "test-token";
    }

    private sealed class FakeAttempts : ILoginAttemptStore
    {
        private bool _blocked;
        public int FailureCount { get; private set; }
        public Task<bool> IsBlockedAsync(string normalizedIdentifier, CancellationToken cancellationToken) => Task.FromResult(_blocked);
        public Task RegisterFailureAsync(string normalizedIdentifier, DateTimeOffset nowUtc, CancellationToken cancellationToken)
        {
            FailureCount++;
            _blocked = FailureCount >= 5;
            return Task.CompletedTask;
        }
        public Task ResetAsync(string normalizedIdentifier, CancellationToken cancellationToken) { FailureCount = 0; _blocked = false; return Task.CompletedTask; }
    }

    private sealed class FakeClock : IClock
    {
        public DateTimeOffset UtcNow => DateTimeOffset.Parse("2026-11-25T12:00:00Z");
    }
}
