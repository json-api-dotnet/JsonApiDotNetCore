using JsonApiDotNetCore.Models.Links;
using System.Collections.Generic;

namespace JsonApiDotNetCore.Models.Fluent
{
    public interface IResourceMapping
    {
        ResourceAttribute Resource { get; }
        LinksAttribute Links { get; }
        List<AttrAttribute> Attributes { get; }
        List<RelationshipAttribute> Relationships { get; }
        List<EagerLoadAttribute> EagerLoads { get; }
    }
}
