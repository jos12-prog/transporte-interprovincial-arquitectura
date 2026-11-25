using System.Text.Json;
using System.Reflection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using System.Threading.RateLimiting;
using Microsoft.OpenApi.Models;
using Serilog;
using TransportPlatform.Modules.AuditNotifications.Infrastructure;
using TransportPlatform.Modules.AuditNotifications.Presentation;
using TransportPlatform.Modules.CompaniesAgencies.Infrastructure;
using TransportPlatform.Modules.CompaniesAgencies.Presentation;
using TransportPlatform.Modules.IdentityAccess.Infrastructure;
using TransportPlatform.Modules.IdentityAccess.Infrastructure.Security;
using TransportPlatform.Modules.IdentityAccess.Presentation;
using TransportPlatform.Modules.IdentityAccess.Presentation.Authorization;
using TransportPlatform.Modules.QueriesReports.Infrastructure;
using TransportPlatform.Modules.QueriesReports.Presentation;
using TransportPlatform.Modules.Reservations.Infrastructure;
using TransportPlatform.Modules.Reservations.Presentation;
using TransportPlatform.Modules.SalesPayments.Infrastructure;
using TransportPlatform.Modules.SalesPayments.Presentation;
using TransportPlatform.Modules.TransportScheduling.Infrastructure;
using TransportPlatform.Modules.TransportScheduling.Presentation;
using TransportPlatform.Shared.Infrastructure;
using TransportPlatform.Host;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UsePlatformSerilog();
builder.Services.AddSharedInfrastructure(builder.Configuration);
builder.Services.AddIdentityAccessInfrastructure(builder.Configuration);
builder.Services.AddAuthentication("Bearer")
    .AddScheme<AuthenticationSchemeOptions, JwtBearerHandler>("Bearer", _ => { });
builder.Services.AddPlatformAuthorization();
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("login-by-ip", httpContext => RateLimitPartition.GetFixedWindowLimiter(
        httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 20,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            AutoReplenishment = true
        }));
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/problem+json";
        context.HttpContext.Response.Headers.RetryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter)
            ? Math.Ceiling(retryAfter.TotalSeconds).ToString(System.Globalization.CultureInfo.InvariantCulture)
            : "60";
        await context.HttpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status429TooManyRequests,
            Title = "Se excedió el límite de solicitudes."
        }, cancellationToken);
    };
});
builder.Services.AddProblemDetails();
builder.Services.AddScoped<IAuthorizationHandler, AgencyScopeAuthorizationHandler>();
builder.Services.AddCompaniesAgenciesInfrastructure(builder.Configuration);
builder.Services.AddTransportSchedulingInfrastructure(builder.Configuration);
builder.Services.AddReservationsInfrastructure(builder.Configuration);
builder.Services.AddSalesPaymentsInfrastructure(builder.Configuration);
builder.Services.AddQueriesReportsInfrastructure(builder.Configuration);
builder.Services.AddAuditNotificationsInfrastructure(builder.Configuration);
builder.Services.AddControllers()
    .AddApplicationPart(typeof(TransportPlatform.Modules.IdentityAccess.Presentation.PresentationAssemblyMarker).Assembly)
    .AddApplicationPart(typeof(TransportPlatform.Modules.CompaniesAgencies.Presentation.PresentationAssemblyMarker).Assembly)
    .AddApplicationPart(typeof(TransportPlatform.Modules.TransportScheduling.Presentation.PresentationAssemblyMarker).Assembly)
    .AddApplicationPart(typeof(TransportPlatform.Modules.Reservations.Presentation.PresentationAssemblyMarker).Assembly)
    .AddApplicationPart(typeof(TransportPlatform.Modules.SalesPayments.Presentation.PresentationAssemblyMarker).Assembly)
    .AddApplicationPart(typeof(TransportPlatform.Modules.QueriesReports.Presentation.PresentationAssemblyMarker).Assembly)
    .AddApplicationPart(typeof(TransportPlatform.Modules.AuditNotifications.Presentation.PresentationAssemblyMarker).Assembly);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Token de acceso JWT"
    });
    options.OperationFilter<TransportPlatform.Host.AuthorizeCheckOperationFilter>();
    var documentedAssemblies = new[]
    {
        Assembly.GetExecutingAssembly(),
        typeof(TransportPlatform.Modules.QueriesReports.Presentation.PresentationAssemblyMarker).Assembly,
        typeof(TransportPlatform.Modules.Reservations.Presentation.PresentationAssemblyMarker).Assembly
        ,typeof(TransportPlatform.Modules.IdentityAccess.Presentation.PresentationAssemblyMarker).Assembly
    };
    foreach (var assembly in documentedAssemblies)
    {
        var xmlPath = Path.ChangeExtension(assembly.Location, ".xml");
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
        }
    }
});

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    await TransportPlatform.Host.DevelopmentDatabaseMigrator.ApplyAsync(app.Services);
}
app.UseSerilogRequestLogging();
app.UseExceptionHandler();
app.UseStatusCodePages(async statusCodeContext =>
{
    var httpContext = statusCodeContext.HttpContext;
    if (httpContext.Response.StatusCode is not (StatusCodes.Status401Unauthorized or StatusCodes.Status403Forbidden))
        return;

    var problemDetailsService = httpContext.RequestServices.GetRequiredService<IProblemDetailsService>();
    var problemDetails = new ProblemDetails
    {
        Status = httpContext.Response.StatusCode,
        Title = httpContext.Response.StatusCode == StatusCodes.Status401Unauthorized
            ? "Se requiere autenticación."
            : "Acceso denegado."
    };
    problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;
    await problemDetailsService.WriteAsync(new ProblemDetailsContext
    {
        HttpContext = httpContext,
        ProblemDetails = problemDetails
    });
});
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapControllers();
app.MapReservationsEndpoints();
app.MapGet("/health", GetHealth)
    .WithName("GetHealth")
    .WithSummary("Comprueba la disponibilidad de PostgreSQL, Redis y RabbitMQ.")
    .WithDescription("Responde 200 si todas las dependencias están saludables o 503 si alguna no lo está; no revela credenciales ni detalles de conexión.")
    .Produces<object>(StatusCodes.Status200OK, "application/json")
    .Produces<object>(StatusCodes.Status503ServiceUnavailable, "application/json");

app.Run();

static async Task<IResult> GetHealth(HealthCheckService healthChecks, CancellationToken cancellationToken)
{
    var report = await healthChecks.CheckHealthAsync(cancellationToken);
    var statusCode = report.Status == HealthStatus.Healthy
        ? StatusCodes.Status200OK
        : StatusCodes.Status503ServiceUnavailable;
    var payload = new
    {
        status = report.Status.ToString().ToLowerInvariant(),
        components = report.Entries.ToDictionary(
            entry => entry.Key,
            entry => entry.Value.Status.ToString().ToLowerInvariant())
    };
    return Results.Json(payload, statusCode: statusCode);
}

/// <summary>Entry point assembly marker for ASP.NET Core integration tests.</summary>
public partial class Program { }
