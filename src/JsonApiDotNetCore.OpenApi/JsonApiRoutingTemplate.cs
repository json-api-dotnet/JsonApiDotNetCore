#pragma warning disable AV1008 // Class should not be static

namespace JsonApiDotNetCore.OpenApi;

internal static class JsonApiRoutingTemplate
{
    public const string RelationshipNameUrlPlaceholder = "{" + JsonApiPathParameter.RelationshipName + "}";
    public const string RelationshipsPart = "relationships";
    public const string PrimaryEndpoint = "{id}";
    public const string SecondaryEndpoint = "{id}/" + RelationshipNameUrlPlaceholder;
    public const string RelationshipEndpoint = "{id}/" + RelationshipsPart + "/" + RelationshipNameUrlPlaceholder;
}
