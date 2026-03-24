using NetArchTest.Rules;

namespace OnionArch.FitFunctions;

/// <summary>
/// Fit functions that validate interface ownership follows onion architecture principles.
/// Interfaces should be owned by the consumer (Application), not the provider (Infrastructure).
/// </summary>
public class InterfaceOwnership
{
    [Fact]
    public void Repository_Interfaces_Should_Be_In_Application_Layer()
    {
        // Arrange & Act
        var result = Types.InNamespace("OnionArch.Application.Interfaces")
            .That()
            .HaveNameEndingWith("Repository")
            .Should()
            .BeInterfaces()
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            "Repository interfaces must be in Application layer (dependency inversion)");
    }

    [Fact]
    public void Infrastructure_Should_Not_Define_Interfaces()
    {
        // Arrange & Act
        var interfaces = Types.InAssembly(typeof(Infrastructure.Persistence.ApplicationDbContext).Assembly)
            .That()
            .ResideInNamespace("OnionArch.Infrastructure")
            .And()
            .AreInterfaces()
            .GetTypes();

        // Assert
        Assert.Empty(interfaces);
    }

    [Fact]
    public void Repository_Implementations_Should_Be_In_Infrastructure()
    {
        // Arrange & Act
        var result = Types.InNamespace("OnionArch.Infrastructure.Repositories")
            .That()
            .HaveNameEndingWith("Repository")
            .Should()
            .BeClasses()
            .And()
            .HaveDependencyOn("OnionArch.Application.Interfaces")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            "Repository implementations must be in Infrastructure and depend on Application interfaces");
    }

    [Fact]
    public void Interfaces_Should_Be_Named_With_I_Prefix()
    {
        // Arrange & Act
        var result = Types.InNamespace("OnionArch.Application.Interfaces")
            .That()
            .AreInterfaces()
            .Should()
            .HaveNameStartingWith("I")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            "Interfaces should follow C# convention of I prefix");
    }
}
