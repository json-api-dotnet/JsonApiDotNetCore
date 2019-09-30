using JsonApiDotNetCore.Models.Links;

namespace JsonApiDotNetCore.Configuration
{
    public interface ILinksConfiguration
    {
        bool RelativeLinks { get; }
        Link RelationshipLinks { get; }
        Link TopLevelLinks { get; }
        Link ResourceLinks { get; }
    }
}