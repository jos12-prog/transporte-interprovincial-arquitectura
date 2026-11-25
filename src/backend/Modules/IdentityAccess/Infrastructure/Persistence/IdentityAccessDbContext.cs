using Microsoft.EntityFrameworkCore;
using TransportPlatform.Modules.IdentityAccess.Domain.Entities;

namespace TransportPlatform.Modules.IdentityAccess.Infrastructure.Persistence;

public sealed class IdentityAccessDbContext(DbContextOptions<IdentityAccessDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<UserAgencyAssignment> UserAgencyAssignments => Set<UserAgencyAssignment>();
    public DbSet<LoginAttemptWindow> LoginAttemptWindows => Set<LoginAttemptWindow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("identity_access");

        var users = modelBuilder.Entity<User>();
        users.ToTable("users");
        users.HasKey(user => user.Id);
        users.Property(user => user.FullName).HasColumnName("full_name").HasMaxLength(200).IsRequired();
        users.Property(user => user.NormalizedDni).HasColumnName("normalized_dni").HasMaxLength(8);
        users.Property(user => user.NormalizedEmail).HasColumnName("normalized_email").HasMaxLength(320).IsRequired();
        users.Property(user => user.PhoneNumber).HasColumnName("phone_number").HasMaxLength(32).IsRequired();
        users.Property(user => user.PasswordHash).HasColumnName("password_hash").HasMaxLength(1024).IsRequired();
        users.Property(user => user.Role).HasColumnName("role").HasMaxLength(32).IsRequired();
        users.HasIndex(user => user.NormalizedDni).IsUnique().HasFilter("normalized_dni IS NOT NULL").HasDatabaseName("UX_users_normalized_dni");
        users.HasIndex(user => user.NormalizedEmail).IsUnique().HasDatabaseName("UX_users_normalized_email");
        users.HasMany(user => user.AgencyAssignments).WithOne().HasForeignKey(assignment => assignment.UserId).OnDelete(DeleteBehavior.Cascade);
        users.Navigation(user => user.AgencyAssignments).HasField("_agencyAssignments").UsePropertyAccessMode(PropertyAccessMode.Field);

        var assignments = modelBuilder.Entity<UserAgencyAssignment>();
        assignments.ToTable("user_agency_assignments");
        assignments.HasKey(assignment => new { assignment.UserId, assignment.AgencyId });
        assignments.Property(assignment => assignment.CompanyId).HasColumnName("company_id").IsRequired();
        assignments.Property(assignment => assignment.AgencyId).HasColumnName("agency_id").IsRequired();
        assignments.HasIndex(assignment => new { assignment.CompanyId, assignment.AgencyId }).HasDatabaseName("IX_user_agency_assignments_company_agency");

        var windows = modelBuilder.Entity<LoginAttemptWindow>();
        windows.ToTable("login_attempt_windows");
        windows.HasKey(window => window.IdentifierHash);
        windows.Property(window => window.IdentifierHash).HasColumnName("identifier_hash").HasMaxLength(64);
        windows.Property(window => window.FailureCount).HasColumnName("failure_count").IsRequired();
        windows.Property(window => window.WindowStartedAtUtc).HasColumnName("window_started_at_utc").IsRequired();
        windows.Property(window => window.BlockedUntilUtc).HasColumnName("blocked_until_utc");
    }
}
