using System;
using System.Linq;
using System.Text.Json;

namespace TestBuildingBlocks
{
    public static class JsonElementExtensions
    {
        public static JsonElementAssertions Should(this JsonElement source)
        {
            return new(source);
        }

        public static string GetReferenceSchemaId(this JsonElement source)
        {
            if (source.ValueKind == JsonValueKind.String)
            {
                try
                {
                    return source.GetString()!.Split('/').Last();
                }
#pragma warning disable AV1210
                catch
#pragma warning restore AV1210
                {
                    throw new InvalidOperationException($"Failed to extract a reference schema id from '{source.GetString()}'");
                }
            }

            throw new ArgumentException($"JsonValueKind of {nameof(source)} should be of JsonValueKind.String");
        }
    }
}
