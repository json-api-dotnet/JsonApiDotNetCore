using JetBrains.Annotations;

namespace JsonApiDotNetCore.Configuration;

[PublicAPI]
public sealed class PageNumber : IEquatable<PageNumber>
{
    public static readonly PageNumber ValueOne = new(1);

    public int OneBasedValue { get; }

    public PageNumber(int oneBasedValue)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(oneBasedValue, 1);

        OneBasedValue = oneBasedValue;
    }

    public bool Equals(PageNumber? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return OneBasedValue == other.OneBasedValue;
    }

    public override bool Equals(object? other)
    {
        return Equals(other as PageNumber);
    }

    public override int GetHashCode()
    {
        return OneBasedValue.GetHashCode();
    }

    public override string ToString()
    {
        return OneBasedValue.ToString();
    }
}
