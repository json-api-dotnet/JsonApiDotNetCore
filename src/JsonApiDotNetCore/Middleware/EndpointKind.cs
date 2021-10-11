namespace JsonApiDotNetCore.Middleware
{
    public enum EndpointKind
    {
        /// <summary>
        /// A top-level resource request, for example: "/blogs" or "/blogs/123"
        /// </summary>
        Primary,

        /// <summary>
        /// A nested resource request, for example: "/blogs/123/author" or "/author/123/articles"
        /// </summary>
        Secondary,

        /// <summary>
        /// A relationship request, for example: "/blogs/123/relationships/author" or "/author/123/relationships/articles"
        /// </summary>
        Relationship,

        /// <summary>
        /// A request to an atomic:operations endpoint.
        /// </summary>
        AtomicOperations
    }
}
