namespace OnionArch.Domain.ValueObjects;

public sealed record Quantity
{
    public int Value { get; }

    private Quantity(int value)
    {
        Value = value;
    }

    public static Quantity Create(int value)
    {
        if (value <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(value));

        return new Quantity(value);
    }

    public Quantity Add(Quantity other) => Create(Value + other.Value);
    public Quantity Subtract(Quantity other) => Create(Value - other.Value);

    public static implicit operator int(Quantity quantity) => quantity.Value;
    public override string ToString() => Value.ToString();
}
