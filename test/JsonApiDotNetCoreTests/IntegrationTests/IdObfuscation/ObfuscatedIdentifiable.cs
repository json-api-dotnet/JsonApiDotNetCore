using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCoreTests.IntegrationTests.IdObfuscation;

public abstract class ObfuscatedIdentifiable : Identifiable<int>
{
    private static readonly HexadecimalCodec Codec = new();

    protected override string? GetStringId(int value)
    {
        return value == 0 ? null : Codec.Encode(value);
    }

    protected override int GetTypedId(string? value)
    {
        return value == null ? 0 : Codec.Decode(value);
    }
}
