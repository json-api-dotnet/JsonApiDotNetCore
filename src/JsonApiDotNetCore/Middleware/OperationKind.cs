namespace JsonApiDotNetCore.Middleware
{
    /// <summary>
    /// Lists the functional operation kinds from a resource request or an atomic:operations request.
    /// </summary>
    public enum OperationKind
    {
        /// <summary>
        /// Create a new resource with attributes, relationships or both.
        /// </summary>
        CreateResource,

        /// <summary>
        /// Update the attributes and/or relationships of an existing resource. Only the values of sent attributes are replaced. And only the values of sent
        /// relationships are replaced.
        /// </summary>
        UpdateResource,

        /// <summary>
        /// Delete an existing resource.
        /// </summary>
        DeleteResource,

        /// <summary>
        /// Perform a complete replacement of a relationship on an existing resource.
        /// </summary>
        SetRelationship,

        /// <summary>
        /// Add resources to a to-many relationship.
        /// </summary>
        AddToRelationship,

        /// <summary>
        /// Remove resources from a to-many relationship.
        /// </summary>
        RemoveFromRelationship
    }
}
