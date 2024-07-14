#pragma warning disable AV1008 // Class should not be static

namespace JsonApiDotNetCore.OpenApi.Swashbuckle;

internal static class JsonApiRoutingTemplate
{
    public const string RelationshipNameRoutePlaceholder = "{" + JsonApiPathParameter.RelationshipName + "}";
    public const string RelationshipsPart = "relationships";
    public const string PrimaryEndpoint = "{id}";
    public const string SecondaryEndpoint = "{id}/" + RelationshipNameRoutePlaceholder;
    public const string RelationshipEndpoint = "{id}/" + RelationshipsPart + "/" + RelationshipNameRoutePlaceholder;
}
