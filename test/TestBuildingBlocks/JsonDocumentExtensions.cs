using System.Text.Json;

namespace TestBuildingBlocks
{
    public static class JsonDocumentExtensions
    {
        public static JsonElement ToJsonElement(this JsonDocument source)
        {
            using (source)
            {
                JsonElement clonedRoot = source.RootElement.Clone();
                return clonedRoot;
            }
        }
    }
}
