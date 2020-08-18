using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Middleware
{
    /// <inheritdoc/>
    public sealed class JsonApiRequest : IJsonApiRequest
    {
        /// <inheritdoc/>
        public EndpointKind Kind { get; set; }
        
        /// <inheritdoc/>
        public string BasePath { get; set; }
        
        /// <inheritdoc/>
        public string PrimaryId { get; set; }
        
        /// <inheritdoc/>
        public ResourceContext PrimaryResource { get; set; }
        
        /// <inheritdoc/>
        public ResourceContext SecondaryResource { get; set; }
        
        /// <inheritdoc/>
        public RelationshipAttribute Relationship { get; set; }
        
        /// <inheritdoc/>
        public bool IsCollection { get; set; }
        
        /// <inheritdoc/>
        public bool IsReadOnly { get; set; }
    }
}
