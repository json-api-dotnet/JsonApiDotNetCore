using System;
using System.Linq;
using System.Text.Json;

#pragma warning disable AV1008 // Class should not be static
#pragma warning disable AV1210 // Catch a specific exception instead of Exception, SystemException or ApplicationException

namespace TestBuildingBlocks
{
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
                throw new Exception($"Failed to extract Error ID from response body '{responseBody}'.", exception);
            }
        }
    }
}
