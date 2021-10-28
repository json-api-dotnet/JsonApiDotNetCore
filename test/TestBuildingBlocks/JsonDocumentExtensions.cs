using System.Text.Json;
using BlushingPenguin.JsonPath;

namespace TestBuildingBlocks
{
    public static class JsonDocumentExtensions
    {
        public static JsonElement SelectTokenOrError(this JsonDocument source, string path)
        {
            return source.SelectToken(path, true)!.Value;
        }
    }
}
