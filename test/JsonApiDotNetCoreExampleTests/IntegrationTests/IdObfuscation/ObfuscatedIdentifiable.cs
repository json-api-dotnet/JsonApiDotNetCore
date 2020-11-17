using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.IdObfuscation
{
    public abstract class ObfuscatedIdentifiable : Identifiable
    {
        protected override string GetStringId(int value)
        {
            return HexadecimalCodec.Encode(value);
        }

        protected override int GetTypedId(string value)
        {
            return HexadecimalCodec.Decode(value);
        }
    }
}
