namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata;

internal sealed class SecondaryResponseMetadata(IDictionary<string, Type> documentTypesByRelationshipName)
    : NonPrimaryEndpointMetadata(documentTypesByRelationshipName), IJsonApiResponseMetadata;
