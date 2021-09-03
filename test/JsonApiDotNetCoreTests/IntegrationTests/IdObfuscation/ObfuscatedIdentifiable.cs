using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCoreTests.IntegrationTests.IdObfuscation
{
    public abstract class ObfuscatedIdentifiable : Identifiable
    {
        private static readonly HexadecimalCodec Codec = new();

        protected override string GetStringId(int value)
        {
            return Codec.Encode(value);
        }

        protected override int GetTypedId(string value)
        {
            return Codec.Decode(value);
        }
    }
}
