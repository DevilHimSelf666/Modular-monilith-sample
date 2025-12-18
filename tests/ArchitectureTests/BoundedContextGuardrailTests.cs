using NetArchTest.Rules;
using Procurement.Domain;
using Inventory.Domain.Entities;
using Xunit;
using System.Linq;
using System.Reflection;

namespace ArchitectureTests;

public class BoundedContextGuardrailTests
{
    [Fact]
    public void ProcurementDomain_IsPure()
    {
        var result = Types.InAssembly(typeof(ProcurementRequest).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(new[]
            {
                "Microsoft.EntityFrameworkCore",
                "MassTransit",
                "Elsa",
                "Microsoft.AspNetCore"
            })
            .GetResult();

        Assert.True(result.IsSuccessful, result.GetOffendingTypes());
    }

    [Fact]
    public void InventoryDomain_IsPure()
    {
        var result = Types.InAssembly(typeof(Warehouse).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(new[]
            {
                "Microsoft.EntityFrameworkCore",
                "MassTransit",
                "Elsa",
                "Microsoft.AspNetCore"
            })
            .GetResult();

        Assert.True(result.IsSuccessful, result.GetOffendingTypes());
    }

    [Fact]
    public void ProcurementProjects_DoNotReference_InventoryImplementations()
    {
        var assemblies = new[]
        {
            typeof(ProcurementRequest).Assembly,
            typeof(Procurement.Application.DependencyInjection).Assembly,
            typeof(Procurement.Infrastructure.Extensions.ServiceCollectionExtensions).Assembly
        };

        var result = Types.InAssemblies(assemblies)
            .ShouldNot()
            .HaveDependencyOnAny(new[]
            {
                "Inventory.Domain",
                "Inventory.Application",
                "Inventory.Infrastructure"
            })
            .GetResult();

        Assert.True(result.IsSuccessful, result.GetOffendingTypes());
    }

    [Fact]
    public void InventoryProjects_DoNotReference_ProcurementInfrastructure()
    {
        var assemblies = new[]
        {
            typeof(Warehouse).Assembly,
            typeof(Inventory.Application.DependencyInjection).Assembly,
            typeof(Inventory.Infrastructure.Extensions.ServiceCollectionExtensions).Assembly
        };

        var result = Types.InAssemblies(assemblies)
            .ShouldNot()
            .HaveDependencyOnAny(new[]
            {
                "Procurement.Infrastructure",
                "Procurement.Application",
                "Procurement.Domain"
            })
            .GetResult();

        Assert.True(result.IsSuccessful, result.GetOffendingTypes());
    }
}

file static class NetArchTestExtensions
{
    public static string GetOffendingTypes(this TestResult result)
    {
        var failing = result.FailingTypes?.Select(t => t.FullName) ?? Enumerable.Empty<string>();
        return string.Join(", ", failing);
    }
}
