using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCoreTests.IntegrationTests.IdObfuscation;

// Tip: Add [HideResourceIdTypeInOpenApi] if you're using OpenAPI with JsonApiDotNetCore.OpenApi.Swashbuckle.
public abstract class ObfuscatedIdentifiable : Identifiable<long>
{
    protected override string? GetStringId(long value)
    {
        return value == 0 ? null : HexadecimalCodec.Instance.Encode(value);
    }

    protected override long GetTypedId(string? value)
    {
        return value == null ? 0 : HexadecimalCodec.Instance.Decode(value);
    }
}
