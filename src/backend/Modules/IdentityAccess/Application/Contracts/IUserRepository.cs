using TransportPlatform.Modules.IdentityAccess.Domain.Entities;

namespace TransportPlatform.Modules.IdentityAccess.Application.Contracts;

public interface IUserRepository
{
    Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<User?> FindByIdentifierAsync(string normalizedIdentifier, CancellationToken cancellationToken);
    Task<bool> ExistsByEmailOrDniAsync(string normalizedEmail, string normalizedDni, CancellationToken cancellationToken);
    Task AddAsync(User user, CancellationToken cancellationToken);
}
