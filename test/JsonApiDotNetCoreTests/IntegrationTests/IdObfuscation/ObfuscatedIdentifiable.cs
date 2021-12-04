using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCoreTests.IntegrationTests.IdObfuscation;

public abstract class ObfuscatedIdentifiable : Identifiable<int>
{
    private static readonly HexadecimalCodec Codec = new();

    protected override string? GetStringId(int value)
    {
        return value == default ? null : Codec.Encode(value);
    }

    protected override int GetTypedId(string? value)
    {
        return value == null ? default : Codec.Decode(value);
    }
}
