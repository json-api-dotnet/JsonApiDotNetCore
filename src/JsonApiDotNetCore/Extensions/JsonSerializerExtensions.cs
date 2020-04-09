using JsonApiDotNetCore.Graph;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace JsonApiDotNetCore.Extensions
{
    internal static class JsonSerializerExtensions
    {
        public static void ApplyErrorSettings(this JsonSerializer jsonSerializer, IResourceNameFormatter formatter)
        {
            jsonSerializer.NullValueHandling = NullValueHandling.Ignore;
            jsonSerializer.ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new NewtonsoftNamingStrategyAdapter(formatter)
            };
        }

        private sealed class NewtonsoftNamingStrategyAdapter : NamingStrategy
        {
            private readonly IResourceNameFormatter _formatter;

            public NewtonsoftNamingStrategyAdapter(IResourceNameFormatter formatter)
            {
                _formatter = formatter;

                ProcessDictionaryKeys = true;
                ProcessExtensionDataNames = true;
            }

            protected override string ResolvePropertyName(string name)
            {
                return _formatter.ApplyCasingConvention(name);
            }
        }
    }
}
