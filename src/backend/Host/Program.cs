using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using TransportPlatform.Modules.AuditNotifications.Infrastructure;
using TransportPlatform.Modules.AuditNotifications.Presentation;
using TransportPlatform.Modules.CompaniesAgencies.Infrastructure;
using TransportPlatform.Modules.CompaniesAgencies.Presentation;
using TransportPlatform.Modules.IdentityAccess.Infrastructure;
using TransportPlatform.Modules.IdentityAccess.Presentation;
using TransportPlatform.Modules.QueriesReports.Infrastructure;
using TransportPlatform.Modules.QueriesReports.Presentation;
using TransportPlatform.Modules.Reservations.Infrastructure;
using TransportPlatform.Modules.Reservations.Presentation;
using TransportPlatform.Modules.SalesPayments.Infrastructure;
using TransportPlatform.Modules.SalesPayments.Presentation;
using TransportPlatform.Modules.TransportScheduling.Infrastructure;
using TransportPlatform.Modules.TransportScheduling.Presentation;
using TransportPlatform.Shared.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UsePlatformSerilog();
builder.Services.AddSharedInfrastructure(builder.Configuration);
builder.Services.AddIdentityAccessInfrastructure(builder.Configuration);
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
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile), includeControllerXmlComments: true);
});

var app = builder.Build();
app.UseSerilogRequestLogging();
app.UseExceptionHandler();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapControllers();
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
