namespace JsonApiDotNetCore.Resources
{
    /// <summary>
    /// Used to determine whether additional changes to a resource (side effects), not specified in a POST or PATCH request, have been applied.
    /// </summary>
    public interface IResourceChangeTracker<in TResource> where TResource : class, IIdentifiable
    {
        /// <summary>
        /// Sets the exposed resource attributes as stored in database, before applying the PATCH operation.
        /// For POST operations, this sets exposed resource attributes to their default value.
        /// </summary>
        void SetInitiallyStoredAttributeValues(TResource resource);

        /// <summary>
        /// Sets the (subset of) exposed resource attributes from the POST or PATCH request.
        /// </summary>
        void SetRequestedAttributeValues(TResource resource);

        /// <summary>
        /// Sets the exposed resource attributes as stored in database, after applying the POST or PATCH operation.
        /// </summary>
        void SetFinallyStoredAttributeValues(TResource resource);

        /// <summary>
        /// Validates if any exposed resource attributes that were not in the POST or PATCH request have been changed.
        /// And validates if the values from the request are stored without modification.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the attribute values from the POST or PATCH request were the only changes; <c>false</c>, otherwise.
        /// </returns>
        bool HasImplicitChanges();
    }
}
