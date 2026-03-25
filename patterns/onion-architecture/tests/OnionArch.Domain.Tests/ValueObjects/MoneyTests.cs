using FluentAssertions;
using OnionArch.Domain.ValueObjects;

namespace OnionArch.Domain.Tests.ValueObjects;

public sealed class MoneyTests
{
    [Fact]
    public void Create_ValidAmountAndCurrency_ReturnsMoney()
    {
        // Arrange & Act
        var money = Money.Create(100.50m, "USD");

        // Assert
        money.Amount.Should().Be(100.50m);
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Create_CurrencyInLowercase_ConvertsToUppercase()
    {
        // Arrange & Act
        var money = Money.Create(100m, "usd");

        // Assert
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Create_NegativeAmount_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => Money.Create(-10m, "USD");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Amount cannot be negative*");
    }

    [Fact]
    public void Create_EmptyCurrency_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => Money.Create(100m, "");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Currency is required*");
    }

    [Fact]
    public void Create_InvalidCurrencyLength_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => Money.Create(100m, "US");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Currency must be 3-letter ISO code*");
    }

    [Fact]
    public void Zero_CreatesMoneyWithZeroAmount()
    {
        // Arrange & Act
        var money = Money.Zero("USD");

        // Assert
        money.Amount.Should().Be(0);
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Add_SameCurrency_ReturnsSum()
    {
        // Arrange
        var money1 = Money.Create(100m, "USD");
        var money2 = Money.Create(50m, "USD");

        // Act
        var result = money1.Add(money2);

        // Assert
        result.Amount.Should().Be(150m);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Add_DifferentCurrency_ThrowsInvalidOperationException()
    {
        // Arrange
        var money1 = Money.Create(100m, "USD");
        var money2 = Money.Create(50m, "EUR");

        // Act
        var act = () => money1.Add(money2);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot add EUR to USD");
    }

    [Fact]
    public void Subtract_SameCurrency_ReturnsDifference()
    {
        // Arrange
        var money1 = Money.Create(100m, "USD");
        var money2 = Money.Create(30m, "USD");

        // Act
        var result = money1.Subtract(money2);

        // Assert
        result.Amount.Should().Be(70m);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Subtract_DifferentCurrency_ThrowsInvalidOperationException()
    {
        // Arrange
        var money1 = Money.Create(100m, "USD");
        var money2 = Money.Create(30m, "EUR");

        // Act
        var act = () => money1.Subtract(money2);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot subtract EUR from USD");
    }

    [Fact]
    public void Multiply_ByFactor_ReturnsProduct()
    {
        // Arrange
        var money = Money.Create(100m, "USD");

        // Act
        var result = money.Multiply(1.5m);

        // Assert
        result.Amount.Should().Be(150m);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void GreaterThan_SameCurrency_ComparesCorrectly()
    {
        // Arrange
        var money1 = Money.Create(100m, "USD");
        var money2 = Money.Create(50m, "USD");

        // Act & Assert
        (money1 > money2).Should().BeTrue();
        (money2 > money1).Should().BeFalse();
    }

    [Fact]
    public void GreaterThan_DifferentCurrency_ThrowsInvalidOperationException()
    {
        // Arrange
        var money1 = Money.Create(100m, "USD");
        var money2 = Money.Create(50m, "EUR");

        // Act
        var act = () => money1 > money2;

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot compare different currencies");
    }

    [Fact]
    public void LessThan_SameCurrency_ComparesCorrectly()
    {
        // Arrange
        var money1 = Money.Create(50m, "USD");
        var money2 = Money.Create(100m, "USD");

        // Act & Assert
        (money1 < money2).Should().BeTrue();
        (money2 < money1).Should().BeFalse();
    }

    [Fact]
    public void ToString_FormatsCorrectly()
    {
        // Arrange
        var money = Money.Create(1234.56m, "USD");

        // Act
        var result = money.ToString();

        // Assert
        result.Should().Be("1,234.56 USD");
    }

    [Fact]
    public void Equals_SameAmountAndCurrency_ReturnsTrue()
    {
        // Arrange
        var money1 = Money.Create(100m, "USD");
        var money2 = Money.Create(100m, "USD");

        // Act & Assert
        money1.Should().Be(money2);
    }

    [Fact]
    public void Equals_DifferentAmount_ReturnsFalse()
    {
        // Arrange
        var money1 = Money.Create(100m, "USD");
        var money2 = Money.Create(50m, "USD");

        // Act & Assert
        money1.Should().NotBe(money2);
    }

    [Fact]
    public void Equals_DifferentCurrency_ReturnsFalse()
    {
        // Arrange
        var money1 = Money.Create(100m, "USD");
        var money2 = Money.Create(100m, "EUR");

        // Act & Assert
        money1.Should().NotBe(money2);
    }
}
