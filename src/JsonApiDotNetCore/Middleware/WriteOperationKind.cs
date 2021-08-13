namespace JsonApiDotNetCore.Middleware
{
    /// <summary>
    /// Lists the functional write operations, originating from a POST/PATCH/DELETE request against a single resource/relationship or a POST request against
    /// a list of operations.
    /// </summary>
    public enum WriteOperationKind
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
