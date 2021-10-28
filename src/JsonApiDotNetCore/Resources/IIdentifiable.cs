namespace JsonApiDotNetCore.Resources
{
    /// <summary>
    /// Defines the basic contract for a JSON:API resource. All resource classes must implement <see cref="IIdentifiable{TId}" />.
    /// </summary>
    public interface IIdentifiable
    {
        /// <summary>
        /// The value for element 'id' in a JSON:API request or response.
        /// </summary>
        string StringId { get; set; }

        /// <summary>
        /// The value for element 'lid' in a JSON:API request.
        /// </summary>
        string LocalId { get; set; }
    }

    /// <summary>
    /// When implemented by a class, indicates to JsonApiDotNetCore that the class represents a JSON:API resource.
    /// </summary>
    /// <typeparam name="TId">
    /// The resource identifier type.
    /// </typeparam>
    public interface IIdentifiable<TId> : IIdentifiable
    {
        /// <summary>
        /// The typed identifier as used by the underlying data store (usually numeric or Guid).
        /// </summary>
        TId Id { get; set; }
    }
}
