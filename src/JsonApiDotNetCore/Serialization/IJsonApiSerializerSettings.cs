using Newtonsoft.Json;

namespace JsonApiDotNetCore.Serialization
{
    public interface IJsonApiSerializerSettings
    {
        JsonSerializerSettings GetSettings();
    }
}
