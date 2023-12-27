namespace JsonApiDotNetCore.OpenApi.JsonApiMetadata;

internal sealed class SecondaryResponseMetadata(IDictionary<string, Type> documentTypesByRelationshipName)
    : NonPrimaryEndpointMetadata(documentTypesByRelationshipName), IJsonApiResponseMetadata;
