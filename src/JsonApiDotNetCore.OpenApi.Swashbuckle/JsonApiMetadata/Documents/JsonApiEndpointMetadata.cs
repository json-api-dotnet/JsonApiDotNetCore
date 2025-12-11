namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata.Documents;

internal sealed class JsonApiEndpointMetadata(IJsonApiRequestMetadata? requestMetadata, IJsonApiResponseMetadata? responseMetadata)
{
    public IJsonApiRequestMetadata? RequestMetadata { get; } = requestMetadata;
    public IJsonApiResponseMetadata? ResponseMetadata { get; } = responseMetadata;
}
