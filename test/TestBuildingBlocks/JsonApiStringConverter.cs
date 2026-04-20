using System.Text.Json;

namespace TestBuildingBlocks;

public static class JsonApiStringConverter
{
    public static string ExtractErrorId(string responseBody)
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse(responseBody);
            return document.RootElement.GetProperty("errors").EnumerateArray().Single()!.GetProperty("id").GetString()!;
        }
        catch (Exception exception)
        {
            throw new JsonException($"Failed to extract Error ID from response body '{responseBody}'.", exception);
        }
    }
}
