using JsonApiDotNetCore.Models;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Serialization
{
    /// <summary>
    /// Options used to configure how fields of a model get serialized into
    /// a json:api <see cref="Document"/>.
    /// </summary>
    public sealed class ResourceObjectBuilderSettings
    {
        public NullValueHandling SerializerNullValueHandling { get; }
        public DefaultValueHandling SerializerDefaultValueHandling { get; }

        public ResourceObjectBuilderSettings(
            NullValueHandling serializerNullValueHandling = NullValueHandling.Include,
            DefaultValueHandling serializerDefaultValueHandling = DefaultValueHandling.Include)
        {
            SerializerNullValueHandling = serializerNullValueHandling;
            SerializerDefaultValueHandling = serializerDefaultValueHandling;
        }
    }
}
