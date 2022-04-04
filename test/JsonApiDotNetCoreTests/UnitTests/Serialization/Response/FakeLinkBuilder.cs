using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCore.Serialization.Response;

namespace JsonApiDotNetCoreTests.UnitTests.Serialization.Response;

internal sealed class FakeLinkBuilder : ILinkBuilder
{
    public TopLevelLinks? GetTopLevelLinks()
    {
        return null;
    }

    public ResourceLinks? GetResourceLinks(ResourceType resourceType, IIdentifiable resource)
    {
        return null;
    }

    public RelationshipLinks? GetRelationshipLinks(RelationshipAttribute relationship, IIdentifiable leftResource)
    {
        return null;
    }
}
