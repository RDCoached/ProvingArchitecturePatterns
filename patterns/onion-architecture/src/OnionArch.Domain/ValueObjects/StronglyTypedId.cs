namespace OnionArch.Domain.ValueObjects;

public abstract record StronglyTypedId<T>(T Value) where T : notnull
{
    public override string ToString() => Value.ToString() ?? string.Empty;
}
