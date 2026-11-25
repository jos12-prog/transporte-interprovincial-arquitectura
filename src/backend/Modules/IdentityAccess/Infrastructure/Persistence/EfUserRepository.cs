using Microsoft.EntityFrameworkCore;
using TransportPlatform.Modules.IdentityAccess.Application.Contracts;
using TransportPlatform.Modules.IdentityAccess.Domain.Entities;

namespace TransportPlatform.Modules.IdentityAccess.Infrastructure.Persistence;

public sealed class EfUserRepository(IdentityAccessDbContext dbContext) : IUserRepository
{
    public Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Users.Include(user => user.AgencyAssignments).SingleOrDefaultAsync(user => user.Id == id, cancellationToken);

    public Task<User?> FindByIdentifierAsync(string normalizedIdentifier, CancellationToken cancellationToken) =>
        dbContext.Users.Include(user => user.AgencyAssignments)
            .SingleOrDefaultAsync(user => user.NormalizedEmail == normalizedIdentifier || user.NormalizedDni == normalizedIdentifier, cancellationToken);

    public Task<bool> ExistsByEmailOrDniAsync(string normalizedEmail, string normalizedDni, CancellationToken cancellationToken) =>
        dbContext.Users.AnyAsync(user => user.NormalizedEmail == normalizedEmail || user.NormalizedDni == normalizedDni, cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken)
    {
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
