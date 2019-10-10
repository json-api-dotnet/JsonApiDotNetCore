using JsonApiDotNetCore.Serialization;

namespace JsonApiDotNetCore.Query
{
    /// <summary>
    /// Encapsulates client overrides of omit null and omit default values behaviour
    /// in <see cref="ResourceObjectBuilderSettings"/>
    /// </summary>
    public interface IAttributeBehaviourService
    {
        /// <summary>
        /// Value of client query param overriding the omit null values behaviour in the server serializer
        /// </summary>
        bool? OmitNullValuedAttributes { get; set; }
        /// <summary>
        /// Value of client query param overriding the omit default values behaviour in the server serializer
        /// </summary>
        bool? OmitDefaultValuedAttributes { get; set; }
    }
}
