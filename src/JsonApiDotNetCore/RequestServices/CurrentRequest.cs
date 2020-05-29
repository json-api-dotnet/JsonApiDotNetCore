using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models.Annotation;
using JsonApiDotNetCore.RequestServices.Contracts;

namespace JsonApiDotNetCore.RequestServices
{
    public sealed class CurrentRequest : ICurrentRequest
    {
        public EndpointKind Kind { get; set; }
        public string BasePath { get; set; }
        public string PrimaryId { get; set; }
        public ResourceContext PrimaryResource { get; set; }
        public ResourceContext SecondaryResource { get; set; }
        public RelationshipAttribute Relationship { get; set; }
        public bool IsCollection { get; set; }
        public bool IsReadOnly { get; set; }
    }
}
