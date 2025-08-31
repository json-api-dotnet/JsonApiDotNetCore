using System.Buffers;
using System.Buffers.Text;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JsonApiDotNetCoreTests.IntegrationTests.IdCompaction;

[JsonConverter(typeof(IdJsonConverter))]
public readonly struct CompactGuid(Guid value) :
    IComparable,
    IComparable<CompactGuid>,
    IEquatable<CompactGuid>,
    ISpanParsable<CompactGuid>,
    IUtf8SpanParsable<CompactGuid>,
    ISpanFormattable,
    IUtf8SpanFormattable
{
    private const int GuidByteSize = 16;
    private const int IdCharMaxSize = 24;

    public static readonly CompactGuid Empty = new(Guid.Empty);

    public static CompactGuid Create()
    {
        return new CompactGuid(Guid.NewGuid());
    }

    /// <inheritdoc />
    public static CompactGuid Parse(string s, IFormatProvider? provider = null)
    {
        ArgumentNullException.ThrowIfNull(s);
        return Parse(s.AsSpan(), provider);
    }

    /// <inheritdoc />
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out CompactGuid result)
    {
        return TryParse(s.AsSpan(), provider, out result);
    }

    /// <inheritdoc />
    public static CompactGuid Parse(ReadOnlySpan<char> s, IFormatProvider? provider = null)
    {
        if (!TryParse(s, provider, out var result))
        {
            throw new ArgumentException(null, nameof(s));
        }

        return result;
    }

    /// <inheritdoc />
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out CompactGuid result)
    {
        Span<byte> charBytes = stackalloc byte[IdCharMaxSize];
        if (!Encoding.ASCII.TryGetBytes(s, charBytes, out var charBytesLength))
        {
            result = default;
            return false;
        }

        return TryParse(charBytes[..charBytesLength], provider, out result);
    }

    /// <inheritdoc />
    public static CompactGuid Parse(ReadOnlySpan<byte> utf8Text, IFormatProvider? provider = null)
    {
        if (!TryParse(utf8Text, provider, out var result))
        {
            throw new ArgumentException(null, nameof(utf8Text));
        }

        return result;
    }

    /// <inheritdoc />
    public static bool TryParse(ReadOnlySpan<byte> utf8Text, IFormatProvider? provider, out CompactGuid result)
    {
        Span<byte> valueBytes = stackalloc byte[GuidByteSize];
        OperationStatus status = Base64.DecodeFromUtf8(utf8Text, valueBytes, out _, out int written);

        if (status != OperationStatus.Done || written < GuidByteSize)
        {
            result = default;
            return false;
        }

        Guid value = new(valueBytes);
        result = new CompactGuid(value);
        return true;
    }

    public static explicit operator Guid(CompactGuid id)
    {
        return id._value;
    }

    public static bool operator ==(CompactGuid id1, CompactGuid id2)
    {
        return id1.Equals(id2);
    }

    public static bool operator !=(CompactGuid id1, CompactGuid id2)
    {
        return !id1.Equals(id2);
    }

    public static bool operator <(CompactGuid id1, CompactGuid id2)
    {
        return id1._value < id2._value;
    }

    public static bool operator >(CompactGuid id1, CompactGuid id2)
    {
        return id1._value > id2._value;
    }

    public static bool operator <=(CompactGuid id1, CompactGuid id2)
    {
        return id1.CompareTo(id2) <= 0;
    }

    public static bool operator >=(CompactGuid id1, CompactGuid id2)
    {
        return id1.CompareTo(id2) >= 0;
    }

    private readonly Guid _value = value;

    /// <inheritdoc />
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return base.Equals(obj);
    }

    /// <inheritdoc />
    public bool Equals(CompactGuid other)
    {
        return _value.Equals(other._value);
    }

    /// <inheritdoc />
    public int CompareTo(object? obj)
    {
        if (obj == null)
        {
            return 1;
        }

        if (obj is CompactGuid other)
        {
            return CompareTo(other);
        }

        throw new ArgumentException(null, nameof(obj));
    }

    /// <inheritdoc />
    public int CompareTo(CompactGuid other)
    {
        return _value.CompareTo(other._value);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        Span<byte> valueBytes = stackalloc byte[GuidByteSize];
        bool ok = _value.TryWriteBytes(valueBytes);
        Debug.Assert(ok);

        Span<byte> charBytes = stackalloc byte[IdCharMaxSize];
        OperationStatus status = Base64.EncodeToUtf8(valueBytes, charBytes, out int consumed, out int written);
        Debug.Assert(status == OperationStatus.Done && consumed == GuidByteSize && written <= IdCharMaxSize);

        return Encoding.ASCII.GetString(charBytes[..written]);
    }

    /// <inheritdoc />
    public string ToString(string? format, IFormatProvider? formatProvider = null)
    {
        return ToString();
    }

    /// <inheritdoc />
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider = null)
    {
        Span<byte> charBytes = stackalloc byte[IdCharMaxSize];
        if (!TryFormat(charBytes, out int bytesWritten, format, provider))
        {
            charsWritten = 0;
            return false;
        }

        Debug.Assert(bytesWritten <= IdCharMaxSize);

        charsWritten = Encoding.ASCII.GetChars(charBytes[..bytesWritten], destination);
        Debug.Assert(charsWritten == bytesWritten);
        return true;
    }

    /// <inheritdoc />
    public bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider = null)
    {
        Span<byte> valueBytes = stackalloc byte[GuidByteSize];
        if (!_value.TryWriteBytes(valueBytes))
        {
            bytesWritten = 0;
            return false;
        }

        OperationStatus status = Base64.EncodeToUtf8(valueBytes, utf8Destination, out int consumed, out bytesWritten);
        Debug.Assert(status == OperationStatus.Done && consumed == GuidByteSize && bytesWritten <= IdCharMaxSize);

        return true;
    }

    private sealed class IdJsonConverter : JsonConverter<CompactGuid>
    {
        public override CompactGuid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                throw new ArgumentException("String expected");
            }

            if (reader.HasValueSequence)
            {
                var seq = reader.ValueSequence;
                return Parse(seq.IsSingleSegment ? seq.FirstSpan : seq.ToArray());
            }

            return Parse(reader.ValueSpan);
        }

        public override void Write(Utf8JsonWriter writer, CompactGuid value, JsonSerializerOptions options)
        {
            Span<byte> idBytes = stackalloc byte[IdCharMaxSize];
            _ = value.TryFormat(idBytes, out _, ReadOnlySpan<char>.Empty);
            writer.WriteStringValue(idBytes);
        }
    }
}
