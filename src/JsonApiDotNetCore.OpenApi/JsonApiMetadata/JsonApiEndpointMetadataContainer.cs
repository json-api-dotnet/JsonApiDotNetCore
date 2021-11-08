namespace JsonApiDotNetCore.OpenApi.JsonApiMetadata
{
    /// <summary>
    /// Metadata available at runtime about a JsonApiDotNetCore endpoint.
    /// </summary>
    internal sealed class JsonApiEndpointMetadataContainer
    {
        public IJsonApiRequestMetadata? RequestMetadata { get; init; }

        public IJsonApiResponseMetadata? ResponseMetadata { get; init; }
    }
}
