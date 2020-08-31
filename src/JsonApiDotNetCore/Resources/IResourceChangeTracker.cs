namespace JsonApiDotNetCore.Resources
{
    /// <summary>
    /// Used to determine whether additional changes to a resource, not specified in a PATCH request, have been applied.
    /// </summary>
    public interface IResourceChangeTracker<in TResource> where TResource : class, IIdentifiable
    {
        /// <summary>
        /// Sets the exposed resource attributes as stored in database, before applying changes.
        /// </summary>
        void SetInitiallyStoredAttributeValues(TResource resource);

        /// <summary>
        /// Sets the subset of exposed attributes from the PATCH request.
        /// </summary>
        void SetRequestedAttributeValues(TResource resource);

        /// <summary>
        /// Sets the exposed resource attributes as stored in database, after applying changes.
        /// </summary>
        void SetFinallyStoredAttributeValues(TResource resource);

        /// <summary>
        /// Validates if any exposed resource attributes that were not in the PATCH request have been changed.
        /// And validates if the values from the PATCH request are stored without modification.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the attribute values from the PATCH request were the only changes; <c>false</c>, otherwise.
        /// </returns>
        bool HasImplicitChanges();
    }
}
