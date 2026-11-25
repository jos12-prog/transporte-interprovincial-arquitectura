using System.Text.RegularExpressions;
using TransportPlatform.Modules.IdentityAccess.Application.Contracts;
using TransportPlatform.Modules.IdentityAccess.Application.Models;
using TransportPlatform.Modules.IdentityAccess.Domain.Entities;
using TransportPlatform.Modules.IdentityAccess.Domain.Roles;

namespace TransportPlatform.Modules.IdentityAccess.Application.UseCases;

public sealed class RegisterPassengerUseCase(IUserRepository users, IPasswordHasher hasher)
{
    public async Task<IdentityResult<Guid>> ExecuteAsync(RegisterPassengerCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.FullName) || string.IsNullOrWhiteSpace(command.PhoneNumber) ||
            !Regex.IsMatch(command.Dni ?? string.Empty, "^\\d{8}$") || !IsEmail(command.Email) ||
            string.IsNullOrWhiteSpace(command.Password) || command.Password.Length < 8)
            return IdentityResult<Guid>.Failed(IdentityFailure.InvalidInput);

        var email = command.Email.Trim().ToUpperInvariant();
        var dni = (command.Dni ?? string.Empty).Trim();
        if (await users.ExistsByEmailOrDniAsync(email, dni, cancellationToken))
            return IdentityResult<Guid>.Failed(IdentityFailure.Conflict);

        var draft = new User(Guid.NewGuid(), command.FullName, dni, email, command.PhoneNumber, "pending", IdentityRoles.Passenger);
        var user = new User(draft.Id, draft.FullName, dni, email, draft.PhoneNumber, hasher.Hash(draft, command.Password), IdentityRoles.Passenger);
        await users.AddAsync(user, cancellationToken);
        return IdentityResult<Guid>.Success(user.Id);
    }

    private static bool IsEmail(string? email) => !string.IsNullOrWhiteSpace(email) &&
        Regex.IsMatch(email.Trim(), "^[^\\s@]+@[^\\s@]+\\.[^\\s@]+$");
}
