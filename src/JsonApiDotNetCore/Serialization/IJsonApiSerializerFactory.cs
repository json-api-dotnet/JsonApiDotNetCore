using JetBrains.Annotations;

namespace JsonApiDotNetCore.Serialization
{
    [PublicAPI]
    public interface IJsonApiSerializerFactory
    {
        /// <summary>
        /// Instantiates the serializer to process the servers response.
        /// </summary>
        IJsonApiSerializer GetSerializer();
    }
}
