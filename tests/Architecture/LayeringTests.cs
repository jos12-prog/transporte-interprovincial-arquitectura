using System.Reflection;
using NetArchTest.Rules;
using Xunit;

namespace TransportPlatform.Architecture.Tests;

public sealed class LayeringTests
{
    private static readonly (string Name, Assembly Domain, Assembly Application)[] Modules =
    [
        ("IdentityAccess", typeof(TransportPlatform.Modules.IdentityAccess.Domain.Class1).Assembly, typeof(TransportPlatform.Modules.IdentityAccess.Application.Class1).Assembly),
        ("CompaniesAgencies", typeof(TransportPlatform.Modules.CompaniesAgencies.Domain.Class1).Assembly, typeof(TransportPlatform.Modules.CompaniesAgencies.Application.Class1).Assembly),
        ("TransportScheduling", typeof(TransportPlatform.Modules.TransportScheduling.Domain.Class1).Assembly, typeof(TransportPlatform.Modules.TransportScheduling.Application.Class1).Assembly),
        ("Reservations", typeof(TransportPlatform.Modules.Reservations.Domain.Class1).Assembly, typeof(TransportPlatform.Modules.Reservations.Application.Class1).Assembly),
        ("SalesPayments", typeof(TransportPlatform.Modules.SalesPayments.Domain.Class1).Assembly, typeof(TransportPlatform.Modules.SalesPayments.Application.Class1).Assembly),
        ("QueriesReports", typeof(TransportPlatform.Modules.QueriesReports.Domain.Class1).Assembly, typeof(TransportPlatform.Modules.QueriesReports.Application.Class1).Assembly),
        ("AuditNotifications", typeof(TransportPlatform.Modules.AuditNotifications.Domain.Class1).Assembly, typeof(TransportPlatform.Modules.AuditNotifications.Application.Class1).Assembly),
    ];

    [Fact]
    public void DomainAssembliesDoNotDependOnInfrastructureFrameworksOrOtherModules()
    {
        foreach (var (name, domain, _) in Modules)
        {
            AssertNoDependency(domain, "Microsoft.EntityFrameworkCore", name);
            AssertNoDependency(domain, "Microsoft.AspNetCore", name);
            AssertNoDependency(domain, $"TransportPlatform.Modules.{name}.Infrastructure", name);
            AssertNoDependency(domain, $"TransportPlatform.Modules.{name}.Presentation", name);
            foreach (var (otherName, _, _) in Modules.Where(module => module.Name != name))
            {
                AssertNoDependency(domain, $"TransportPlatform.Modules.{otherName}", name);
            }
        }
    }

    [Fact]
    public void ApplicationAssembliesDoNotDependOnInfrastructurePresentationOrOtherModules()
    {
        foreach (var (name, _, application) in Modules)
        {
            AssertNoDependency(application, $"TransportPlatform.Modules.{name}.Infrastructure", name);
            AssertNoDependency(application, $"TransportPlatform.Modules.{name}.Presentation", name);
            foreach (var (otherName, _, _) in Modules.Where(module => module.Name != name))
            {
                AssertNoDependency(application, $"TransportPlatform.Modules.{otherName}", name);
            }
        }
    }

    private static void AssertNoDependency(Assembly assembly, string dependency, string module)
    {
        var result = Types.InAssembly(assembly)
            .Should()
            .NotHaveDependencyOn(dependency)
            .GetResult();

        Assert.True(result.IsSuccessful, $"{module} ({assembly.GetName().Name}) depends on '{dependency}'.");
    }
}
