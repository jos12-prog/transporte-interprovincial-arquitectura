using TransportPlatform.Modules.IdentityAccess.Domain.Entities;

namespace TransportPlatform.Modules.IdentityAccess.Application.Models;

public enum IdentityFailure { InvalidInput, Conflict, InvalidCredentials, RateLimited, Forbidden }

public sealed record IdentityResult<T>(T? Value, IdentityFailure? Failure = null, string? Detail = null)
{
    public bool Succeeded => Failure is null;
    public static IdentityResult<T> Success(T value) => new(value);
    public static IdentityResult<T> Failed(IdentityFailure failure, string? detail = null) => new(default, failure, detail);
}

public sealed record RegisterPassengerCommand(string FullName, string Dni, string Email, string PhoneNumber, string Password);
public sealed record LoginCommand(string Identifier, string Password);
public sealed record AccessToken(string Token, DateTimeOffset ExpiresAtUtc, User User);
public sealed record CurrentUser(Guid Id, string FullName, string Email, string Role, Guid? CompanyId, IReadOnlyList<Guid> AgencyIds);
