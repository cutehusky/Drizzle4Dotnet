namespace Drizzle4Dotnet.Shared;

public readonly struct Optional<T>
{
    private readonly bool _hasValue;
    private readonly T _value;

    public T Value => _value;
    public bool HasValue => _hasValue;

    public Optional(T value)
    {
        _value = value;
        _hasValue = true;
    }

    public static implicit operator Optional<T>(T value) => new Optional<T>(value);
    
    public static readonly Optional<T> Undefined = default;
}
