namespace JsonApiDotNetCore.OpenApi.JsonApiMetadata;

/// <summary>
/// Metadata available at runtime about a JsonApiDotNetCore endpoint.
/// </summary>
internal sealed class JsonApiEndpointMetadataContainer(IJsonApiRequestMetadata? requestMetadata, IJsonApiResponseMetadata? responseMetadata)
{
    public IJsonApiRequestMetadata? RequestMetadata { get; } = requestMetadata;
    public IJsonApiResponseMetadata? ResponseMetadata { get; } = responseMetadata;
}
