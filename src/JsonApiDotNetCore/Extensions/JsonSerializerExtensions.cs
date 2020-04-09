using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace JsonApiDotNetCore.Extensions
{
    internal static class JsonSerializerExtensions
    {
        public static void ApplyErrorSettings(this JsonSerializer jsonSerializer)
        {
            jsonSerializer.NullValueHandling = NullValueHandling.Ignore;

            var contractResolver = (DefaultContractResolver)jsonSerializer.ContractResolver;
            contractResolver.NamingStrategy.ProcessDictionaryKeys = true;
            contractResolver.NamingStrategy.ProcessExtensionDataNames = true;
        }
    }
}
