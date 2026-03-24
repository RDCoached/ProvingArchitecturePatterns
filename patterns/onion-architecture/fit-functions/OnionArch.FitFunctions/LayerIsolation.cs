using NetArchTest.Rules;

namespace OnionArch.FitFunctions;

/// <summary>
/// Fit functions that verify each layer is properly isolated with appropriate visibility.
/// </summary>
public class LayerIsolation
{
    [Fact]
    public void Domain_Entities_Should_Be_Public()
    {
        // Arrange & Act
        var result = Types.InNamespace("OnionArch.Domain.Entities")
            .Should()
            .BePublic()
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            "Domain entities should be public to be used by other layers");
    }

    [Fact]
    public void Application_Interfaces_Should_Be_Public()
    {
        // Arrange & Act
        var result = Types.InNamespace("OnionArch.Application.Interfaces")
            .Should()
            .BePublic()
            .And().BeInterfaces()
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            "Application interfaces must be public for Infrastructure to implement them");
    }

    [Fact]
    public void Infrastructure_Implementations_Should_Not_Be_Public()
    {
        // Arrange & Act
        var result = Types.InNamespace("OnionArch.Infrastructure.Repositories")
            .That()
            .HaveNameEndingWith("Repository")
            .Should()
            .BeSealed()
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            "Infrastructure implementations should be sealed (not designed for inheritance)");
    }

    [Fact]
    public void Value_Objects_Should_Be_Sealed()
    {
        // Arrange & Act
        var result = Types.InNamespace("OnionArch.Domain.ValueObjects")
            .That()
            .AreNotAbstract()  // Exclude abstract base classes like StronglyTypedId<T>
            .Should()
            .BeSealed()
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"Concrete value objects should be sealed. Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }
}
