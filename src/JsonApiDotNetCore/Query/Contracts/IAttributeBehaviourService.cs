using JsonApiDotNetCore.Serialization;

namespace JsonApiDotNetCore.Query
{
    /// <summary>
    /// Encapsulates client overrides of omit null and omit default values behaviour
    /// in <see cref="SerializerSettings"/>
    /// </summary>
    public interface IAttributeBehaviourService: IQueryParameterService
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
