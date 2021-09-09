using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#pragma warning disable AV1008 // Class should not be static

namespace TestBuildingBlocks
{
    public static class JsonApiStringConverter
    {
        public static string ExtractErrorId(string responseBody)
        {
            var jObject = JsonConvert.DeserializeObject<JObject>(responseBody);

            JToken jArray = jObject?["errors"];

            if (jArray != null)
            {
                return jArray.Select(element => (string)element["id"]).Single();
            }

            throw new Exception($"Failed to extract Error ID from response body '{responseBody}'.");
        }
    }
}
