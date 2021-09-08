using System;

namespace JsonApiDotNetCore.OpenApi.JsonApiMetadata
{
    internal sealed class PrimaryRequestMetadata : IJsonApiRequestMetadata
    {
        public Type Type { get; init; }
    }
}
