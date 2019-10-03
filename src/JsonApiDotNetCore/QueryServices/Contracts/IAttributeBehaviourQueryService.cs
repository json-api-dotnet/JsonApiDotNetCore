using JsonApiDotNetCore.Serialization.Serializer;

namespace JsonApiDotNetCore.QueryServices.Contracts
{
    /// <summary>
    /// Encapsulates client overrides of omit null and omit default values behaviour
    /// in <see cref="SerializerSettings"/>
    /// </summary>
    public interface IAttributeBehaviourQueryService
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
