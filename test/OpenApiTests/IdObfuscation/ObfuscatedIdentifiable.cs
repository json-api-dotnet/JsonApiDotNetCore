using JsonApiDotNetCore.OpenApi.Swashbuckle.Annotations;
using JsonApiDotNetCore.Resources;

namespace OpenApiTests.IdObfuscation;

[HideResourceIdTypeInOpenApi]
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
