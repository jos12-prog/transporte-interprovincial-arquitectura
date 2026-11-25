using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TransportPlatform.Modules.IdentityAccess.Infrastructure.Persistence;
using TransportPlatform.Modules.IdentityAccess.Infrastructure.Security;
using TransportPlatform.Modules.IdentityAccess.Application.Authorization;
using TransportPlatform.Modules.IdentityAccess.Application.UseCases;
using TransportPlatform.Modules.IdentityAccess.Application.Contracts;
using TransportPlatform.Modules.IdentityAccess.Domain.Entities;
using AspNetPasswordHasherContract = Microsoft.AspNetCore.Identity.IPasswordHasher<TransportPlatform.Modules.IdentityAccess.Domain.Entities.User>;
using AspNetPasswordHasherImplementation = Microsoft.AspNetCore.Identity.PasswordHasher<TransportPlatform.Modules.IdentityAccess.Domain.Entities.User>;

namespace TransportPlatform.Modules.IdentityAccess.Infrastructure;

/// <summary>Registers persistence for the IdentityAccess module.</summary>
public static class InfrastructureRegistration
{
    public static IServiceCollection AddIdentityAccessInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["POSTGRES_CONNECTION_STRING"]
            ?? "Host=postgres;Port=5432;Database=transport;Username=transport;Password=local-development-only";
        services.AddDbContext<IdentityAccessDbContext>(options => options.UseNpgsql(connectionString, postgres =>
            postgres.MigrationsHistoryTable("__EFMigrationsHistory", "identity_access")));
        services.AddScoped<IUserRepository, EfUserRepository>();
        var hmacKey = configuration["LOGIN_ATTEMPT_HMAC_KEY"];
        var identifierHasher = new HmacIdentifierHasher(hmacKey ?? string.Empty);
        services.AddSingleton(identifierHasher);
        services.AddScoped<ILoginAttemptStore, EfLoginAttemptStore>();
        services.AddSingleton<AspNetPasswordHasherContract, AspNetPasswordHasherImplementation>();
        services.AddSingleton<IPasswordHasher, AspNetPasswordHasher>();
        var issuer = configuration["JWT_ISSUER"];
        var privateKeyPath = configuration["JWT_SIGNING_PRIVATE_KEY_PATH"];
        var publicKeyPath = configuration["JWT_SIGNING_PUBLIC_KEY_PATH"];
        if (string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(privateKeyPath) || string.IsNullOrWhiteSpace(publicKeyPath))
            throw new InvalidOperationException("JWT issuer and signing key paths must be configured.");
        var tokens = new JwtAccessTokenService(issuer, File.ReadAllText(privateKeyPath), File.ReadAllText(publicKeyPath));
        services.AddSingleton(tokens);
        services.AddSingleton<IAccessTokenIssuer>(tokens);
        services.AddSingleton<IAccessTokenValidator>(tokens);
        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<RegisterPassengerUseCase>();
        services.AddScoped<LoginUseCase>();
        services.AddScoped<GetCurrentUserUseCase>();
        services.AddSingleton<AgencyScope>();
        return services;
    }
}
