namespace JsonApiDotNetCore.Resources
{
    /// <summary>
    /// When implemented by a class, indicates to JsonApiDotNetCore that the class represents a json:api resource.
    /// Note that JsonApiDotNetCore also assumes that a property named 'Id' exists.
    /// </summary>
    public interface IIdentifiable
    {
        /// <summary>
        /// The value for element 'id' in a json:api request or response.
        /// </summary>
        string StringId { get; set; }
    }

    /// <summary>
    /// When implemented by a class, indicates to JsonApiDotNetCore that the class represents a json:api resource.
    /// </summary>
    /// <typeparam name="TId">The resource identifier type.</typeparam>
    public interface IIdentifiable<TId> : IIdentifiable
    {
        /// <summary>
        /// The typed identifier as used by the underlying data store (usually numeric).
        /// </summary>
        TId Id { get; set; }
    }
}
