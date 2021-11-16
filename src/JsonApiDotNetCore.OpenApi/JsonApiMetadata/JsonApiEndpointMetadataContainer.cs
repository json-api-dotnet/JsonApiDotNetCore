namespace JsonApiDotNetCore.OpenApi.JsonApiMetadata
{
    /// <summary>
    /// Metadata available at runtime about a JsonApiDotNetCore endpoint.
    /// </summary>
    internal sealed class JsonApiEndpointMetadataContainer
    {
        public IJsonApiRequestMetadata? RequestMetadata { get; }

        public IJsonApiResponseMetadata? ResponseMetadata { get; }

        public JsonApiEndpointMetadataContainer(IJsonApiRequestMetadata? requestMetadata, IJsonApiResponseMetadata? responseMetadata)
        {
            RequestMetadata = requestMetadata;
            ResponseMetadata = responseMetadata;
        }
    }
}
