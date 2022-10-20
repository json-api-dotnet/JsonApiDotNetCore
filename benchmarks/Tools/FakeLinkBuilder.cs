using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCore.Serialization.Response;
using Microsoft.AspNetCore.Http;

namespace Benchmarks.Tools;

/// <summary>
/// Renders hard-coded fake links, without depending on <see cref="HttpContext" />.
/// </summary>
internal sealed class FakeLinkBuilder : ILinkBuilder
{
    public TopLevelLinks GetTopLevelLinks()
    {
        return new TopLevelLinks
        {
            Self = "TopLevel:Self"
        };
    }

    public ResourceLinks GetResourceLinks(ResourceType resourceType, IIdentifiable resource)
    {
        return new ResourceLinks
        {
            Self = "Resource:Self"
        };
    }

    public RelationshipLinks GetRelationshipLinks(RelationshipAttribute relationship, IIdentifiable leftResource)
    {
        return new RelationshipLinks
        {
            Self = "Relationship:Self",
            Related = "Relationship:Related"
        };
    }
}
