using System;

namespace JsonApiDotNetCore.OpenApi.JsonApiMetadata
{
    internal sealed class PrimaryResponseMetadata : IJsonApiResponseMetadata
    {
        public Type Type { get; init; }
    }
}
