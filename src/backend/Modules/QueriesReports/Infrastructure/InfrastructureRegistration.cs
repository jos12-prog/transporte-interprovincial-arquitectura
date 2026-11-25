using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TransportPlatform.Modules.QueriesReports.Application.Contracts;
using TransportPlatform.Modules.QueriesReports.Application.UseCases;
using TransportPlatform.Modules.QueriesReports.Application.Services;
using TransportPlatform.Modules.QueriesReports.Infrastructure.Caching;
using TransportPlatform.Modules.QueriesReports.Infrastructure.Persistence;
using TransportPlatform.Shared.Contracts.Modules;

namespace TransportPlatform.Modules.QueriesReports.Infrastructure;

/// <summary>Registers persistence for the QueriesReports module.</summary>
public static class InfrastructureRegistration
{
    public static IServiceCollection AddQueriesReportsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["POSTGRES_CONNECTION_STRING"]
            ?? "Host=postgres;Port=5432;Database=transport;Username=transport;Password=local-development-only";
        services.AddDbContext<QueriesReportsDbContext>(options => options.UseNpgsql(connectionString));
        var redisHost = configuration["REDIS_HOST"] ?? "redis";
        var redisPort = configuration["REDIS_PORT"] ?? "6379";
        services.AddStackExchangeRedisCache(options => options.Configuration = $"{redisHost}:{redisPort},abortConnect=false,connectTimeout=500,syncTimeout=500,asyncTimeout=500,connectRetry=0");
        services.AddSingleton<ITripSearchCache, RedisTripSearchCache>();
        services.AddScoped<ISearchTripsUseCase, SearchTripsUseCase>();
        services.AddScoped<IQueriesReportsCacheControl, QueriesReportsCacheControl>();
        return services;
    }
}
