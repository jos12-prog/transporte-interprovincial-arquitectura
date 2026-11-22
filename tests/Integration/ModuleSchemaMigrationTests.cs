using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Testcontainers.PostgreSql;
using TransportPlatform.Modules.CompaniesAgencies.Infrastructure.Persistence;
using TransportPlatform.Modules.Reservations.Infrastructure.Persistence;
using TransportPlatform.Modules.TransportScheduling.Infrastructure.Persistence;
using Xunit;

namespace TransportPlatform.Integration.Tests;

public sealed class ModuleSchemaMigrationTests
{
    [Fact]
    public async Task ModuleMigrationsCanApplyAndRevertWithoutCrossSchemaForeignKeys()
    {
        await using var postgres = new PostgreSqlBuilder()
            .WithImage("postgres:16")
            .WithDatabase("tripsearch")
            .WithUsername("test")
            .WithPassword("test-password")
            .Build();
        await postgres.StartAsync();

        var connectionString = postgres.GetConnectionString();
        await using var companies = new CompaniesAgenciesDbContext(
            new DbContextOptionsBuilder<CompaniesAgenciesDbContext>()
                .UseNpgsql(connectionString, options => options.MigrationsHistoryTable(
                    "__EFMigrationsHistory", "companies_agencies"))
                .Options);
        await using var scheduling = new TransportSchedulingDbContext(
            new DbContextOptionsBuilder<TransportSchedulingDbContext>()
                .UseNpgsql(connectionString, options => options.MigrationsHistoryTable(
                    "__EFMigrationsHistory", "transport_scheduling"))
                .Options);
        await using var reservations = new ReservationsDbContext(
            new DbContextOptionsBuilder<ReservationsDbContext>()
                .UseNpgsql(connectionString, options => options.MigrationsHistoryTable(
                    "__EFMigrationsHistory", "reservations"))
                .Options);

        await companies.Database.MigrateAsync();
        await scheduling.Database.MigrateAsync();
        await reservations.Database.MigrateAsync();

        Assert.NotEmpty(await companies.Database.GetAppliedMigrationsAsync());
        Assert.NotEmpty(await scheduling.Database.GetAppliedMigrationsAsync());
        Assert.NotEmpty(await reservations.Database.GetAppliedMigrationsAsync());
        Assert.Equal(1, await CountTableAsync(companies, "companies_agencies", "companies"));
        Assert.Equal(1, await CountTableAsync(scheduling, "transport_scheduling", "trips"));
        Assert.Equal(1, await CountTableAsync(reservations, "reservations", "trip_seats"));
        const string requiredBtreeIndexes = """
            SELECT COUNT(*)::int AS "Value"
            FROM pg_indexes
            WHERE indexdef ILIKE '%USING btree%'
              AND ((schemaname = 'transport_scheduling' AND indexname = 'IX_routes_origin_city_destination_city')
                OR (schemaname = 'transport_scheduling' AND indexname = 'IX_trips_route_id_departure_at_utc')
                OR (schemaname = 'reservations' AND indexname = 'IX_trip_seats_trip_id_status'))
            """;
        Assert.Equal(3, await companies.Database.SqlQueryRaw<int>(requiredBtreeIndexes).SingleAsync());

        const string crossSchemaForeignKeys = """
            SELECT COUNT(*)::int AS "Value"
            FROM pg_constraint AS constraint_info
            JOIN pg_class AS source_table ON source_table.oid = constraint_info.conrelid
            JOIN pg_namespace AS source_schema ON source_schema.oid = source_table.relnamespace
            JOIN pg_class AS target_table ON target_table.oid = constraint_info.confrelid
            JOIN pg_namespace AS target_schema ON target_schema.oid = target_table.relnamespace
            WHERE constraint_info.contype = 'f'
              AND source_schema.nspname IN ('companies_agencies', 'transport_scheduling', 'reservations')
              AND target_schema.nspname <> source_schema.nspname
            """;
        Assert.Equal(0, await companies.Database.SqlQueryRaw<int>(crossSchemaForeignKeys).SingleAsync());

        await reservations.Database.GetService<IMigrator>().MigrateAsync("0");
        await scheduling.Database.GetService<IMigrator>().MigrateAsync("0");
        await companies.Database.GetService<IMigrator>().MigrateAsync("0");
        await companies.Database.MigrateAsync();
        await scheduling.Database.MigrateAsync();
        await reservations.Database.MigrateAsync();
    }

    private static Task<int> CountTableAsync(DbContext context, string schema, string table)
    {
        var sql = $"SELECT COUNT(*)::int AS \"Value\" FROM information_schema.tables " +
                  $"WHERE table_schema = '{schema}' AND table_name = '{table}'";
        return context.Database.SqlQueryRaw<int>(sql).SingleAsync();
    }
}
