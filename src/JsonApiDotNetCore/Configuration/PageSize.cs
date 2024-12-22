using JetBrains.Annotations;

namespace JsonApiDotNetCore.Configuration;

[PublicAPI]
public sealed class PageSize : IEquatable<PageSize>
{
    public int Value { get; }

    public PageSize(int value)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);

        Value = value;
    }

    public bool Equals(PageSize? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Value == other.Value;
    }

    public override bool Equals(object? other)
    {
        return Equals(other as PageSize);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
