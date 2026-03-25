using NetArchTest.Rules;

namespace OnionArch.FitFunctions;

/// <summary>
/// Fit functions that ensure the domain layer remains pure and framework-agnostic.
/// </summary>
public class DomainPurity
{
    [Fact]
    public void Domain_Should_Not_Have_EF_Core_Attributes()
    {
        // Arrange & Act
        var result = Types.InNamespace("OnionArch.Domain")
            .Should()
            .NotHaveDependencyOn("System.ComponentModel.DataAnnotations")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            "Domain should not use data annotations (persistence concern)");
    }

    [Fact]
    public void Domain_Entities_Should_Have_Private_Setters()
    {
        // Arrange
        var entityTypes = typeof(Domain.Entities.Order).Assembly
            .GetTypes()
            .Where(t => t.Namespace == "OnionArch.Domain.Entities" && t.IsClass && !t.IsAbstract)
            .ToList();

        var violations = new List<string>();

        // Act
        foreach (var entityType in entityTypes)
        {
            var properties = entityType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            foreach (var property in properties)
            {
                var setter = property.GetSetMethod();

                // Property has a public setter - violation!
                if (setter != null && setter.IsPublic)
                {
                    violations.Add($"{entityType.Name}.{property.Name} has public setter");
                }
            }
        }

        // Assert
        Assert.Empty(violations);
    }

    [Fact]
    public void Domain_Should_Not_Reference_Infrastructure_Concerns()
    {
        // Arrange & Act
        var result = Types.InNamespace("OnionArch.Domain")
            .Should()
            .NotHaveDependencyOnAny(
                "Microsoft.EntityFrameworkCore",
                "Npgsql",
                "Microsoft.AspNetCore",
                "System.Data",
                "Dapper")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"Domain must be infrastructure-agnostic. Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Domain_Entities_Should_Be_In_Correct_Namespace()
    {
        // Arrange & Act
        var result = Types.InAssembly(typeof(Domain.Entities.Order).Assembly)
            .That()
            .ResideInNamespace("OnionArch.Domain.Entities")
            .Should()
            .BeClasses()
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"All types in Entities namespace should be classes. " +
            $"Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }
}
