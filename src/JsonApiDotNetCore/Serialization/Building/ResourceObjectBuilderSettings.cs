using JetBrains.Annotations;
using JsonApiDotNetCore.Serialization.Objects;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Serialization.Building
{
    /// <summary>
    /// Options used to configure how fields of a model get serialized into a JSON:API <see cref="Document" />.
    /// </summary>
    [PublicAPI]
    public sealed class ResourceObjectBuilderSettings
    {
        public NullValueHandling SerializerNullValueHandling { get; }
        public DefaultValueHandling SerializerDefaultValueHandling { get; }

        public ResourceObjectBuilderSettings(NullValueHandling serializerNullValueHandling = NullValueHandling.Include,
            DefaultValueHandling serializerDefaultValueHandling = DefaultValueHandling.Include)
        {
            SerializerNullValueHandling = serializerNullValueHandling;
            SerializerDefaultValueHandling = serializerDefaultValueHandling;
        }
    }
}
