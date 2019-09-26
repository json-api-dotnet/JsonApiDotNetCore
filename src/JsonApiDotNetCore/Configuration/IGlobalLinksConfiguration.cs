using JsonApiDotNetCore.Models.Links;

namespace JsonApiDotNetCore.Configuration
{
    public interface IGlobalLinksConfiguration
    {
        bool RelativeLinks { get; }
        Link RelationshipLinks { get; }
        Link TopLevelLinks { get; }
        Link ResourceLinks { get; }
    }
}