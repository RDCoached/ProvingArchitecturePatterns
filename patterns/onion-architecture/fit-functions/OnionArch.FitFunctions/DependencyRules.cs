using NetArchTest.Rules;
using OnionArch.Application;
using OnionArch.Domain;
using OnionArch.Infrastructure;

namespace OnionArch.FitFunctions;

/// <summary>
/// Fit functions that validate dependency flow in onion architecture.
/// Dependencies must flow inward only: Api → Infrastructure → Application → Domain
/// </summary>
public class DependencyRules
{
    private static readonly System.Reflection.Assembly DomainAssembly = typeof(Domain.Entities.Order).Assembly;
    private static readonly System.Reflection.Assembly ApplicationAssembly = typeof(Application.Interfaces.IOrderRepository).Assembly;
    private static readonly System.Reflection.Assembly InfrastructureAssembly = typeof(Infrastructure.Persistence.ApplicationDbContext).Assembly;

    [Fact]
    public void Domain_Should_Not_Depend_On_Any_Other_Layer()
    {
        // Arrange & Act
        var result = Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOn("OnionArch.Application")
            .And().NotHaveDependencyOn("OnionArch.Infrastructure")
            .And().NotHaveDependencyOn("OnionArch.Api")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"Domain layer should have no dependencies on other layers. Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Domain_Should_Only_Reference_System_Libraries()
    {
        // Arrange & Act
        var result = Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOnAny(
                "Microsoft.EntityFrameworkCore",
                "Npgsql",
                "Newtonsoft.Json",
                "System.Data")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"Domain should not reference infrastructure libraries. Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Application_Should_Not_Depend_On_Infrastructure_Or_Api()
    {
        // Arrange & Act
        var result = Types.InAssembly(ApplicationAssembly)
            .Should()
            .NotHaveDependencyOn("OnionArch.Infrastructure")
            .And().NotHaveDependencyOn("OnionArch.Api")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"Application layer should not depend on Infrastructure or API. Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Application_Can_Only_Depend_On_Domain()
    {
        // Arrange & Act
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ResideInNamespace("OnionArch.Application")
            .Should()
            .NotHaveDependencyOnAny(
                "Microsoft.EntityFrameworkCore",
                "Npgsql",
                "Microsoft.AspNetCore")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"Application should only depend on Domain, not infrastructure. Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Infrastructure_Should_Not_Depend_On_Api()
    {
        // Arrange & Act
        var result = Types.InAssembly(InfrastructureAssembly)
            .Should()
            .NotHaveDependencyOn("OnionArch.Api")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"Infrastructure should not depend on API layer. Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Infrastructure_Must_Depend_On_Application()
    {
        // Arrange & Act
        var result = Types.InAssembly(InfrastructureAssembly)
            .That()
            .HaveNameEndingWith("Repository")
            .Should()
            .HaveDependencyOn("OnionArch.Application")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            "Infrastructure repositories must depend on Application (for interfaces)");
    }
}
