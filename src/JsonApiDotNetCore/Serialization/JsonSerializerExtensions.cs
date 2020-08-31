using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace JsonApiDotNetCore.Serialization
{
    internal static class JsonSerializerExtensions
    {
        public static void ApplyErrorSettings(this JsonSerializer jsonSerializer)
        {
            jsonSerializer.NullValueHandling = NullValueHandling.Ignore;

            // JsonSerializer.Create() only performs a shallow copy of the shared settings, so we cannot change properties on its ContractResolver.
            // But to serialize ErrorMeta.Data correctly, we need to ensure that JsonSerializer.ContractResolver.NamingStrategy.ProcessExtensionDataNames
            // is set to 'true' while serializing errors.
            var sharedContractResolver = (DefaultContractResolver)jsonSerializer.ContractResolver;

            jsonSerializer.ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new AlwaysProcessExtensionDataNamingStrategyWrapper(sharedContractResolver.NamingStrategy)
            };
        }

        private sealed class AlwaysProcessExtensionDataNamingStrategyWrapper : NamingStrategy
        {
            private readonly NamingStrategy _namingStrategy;

            public AlwaysProcessExtensionDataNamingStrategyWrapper(NamingStrategy namingStrategy)
            {
                _namingStrategy = namingStrategy ?? new DefaultNamingStrategy();
            }

            public override string GetExtensionDataName(string name)
            {
                // Ignore the value of ProcessExtensionDataNames property on the wrapped strategy (short-circuit).
                return ResolvePropertyName(name);
            }

            public override string GetDictionaryKey(string key)
            {
                // Ignore the value of ProcessDictionaryKeys property on the wrapped strategy (short-circuit).
                return ResolvePropertyName(key);
            }

            public override string GetPropertyName(string name, bool hasSpecifiedName)
            {
                return _namingStrategy.GetPropertyName(name, hasSpecifiedName);
            }

            protected override string ResolvePropertyName(string name)
            {
                return _namingStrategy.GetPropertyName(name, false);
            }
        }
    }
}
