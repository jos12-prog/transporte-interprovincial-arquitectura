using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using TransportPlatform.Modules.IdentityAccess.Presentation.Authorization;

namespace TransportPlatform.Host;

public static class PlatformAuthorizationRegistration
{
    public static IServiceCollection AddPlatformAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("SuperAdmin", policy => policy.RequireAuthenticatedUser().RequireRole("super_admin"));
            options.AddPolicy("StaffAgencies", policy => policy.RequireAuthenticatedUser().RequireRole("agent", "agency_admin"));
            options.AddPolicy("AgencyScope", policy => policy.RequireAuthenticatedUser().AddRequirements(new AgencyScopeRequirement()));
        });
        return services;
    }
}
