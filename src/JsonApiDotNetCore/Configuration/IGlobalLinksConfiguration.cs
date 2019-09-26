using JsonApiDotNetCore.Models.Links;

namespace JsonApiDotNetCore.Configuration
{
    public interface IGlobalLinksConfiguration
    {
        bool RelativeLinks { get; set; }
        Link RelationshipLinks { get; set; }
        Link TopLevelLinks { get; set; }
        Link ResourceLinks { get; set; }
    }
}