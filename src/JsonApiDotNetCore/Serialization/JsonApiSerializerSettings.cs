using Newtonsoft.Json;

namespace JsonApiDotNetCore.Serialization
{
    public class JsonApiSerializerSettings : IJsonApiSerializerSettings
    {
        public JsonSerializerSettings GetSettings()
        {
            return new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new DasherizedResolver(),
                DateParseHandling = DateParseHandling.None
            };
        }

    }
}
