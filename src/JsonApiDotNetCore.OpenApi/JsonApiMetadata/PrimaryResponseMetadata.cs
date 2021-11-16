using System;

namespace JsonApiDotNetCore.OpenApi.JsonApiMetadata
{
    internal sealed class PrimaryResponseMetadata : IJsonApiResponseMetadata
    {
        public Type Type { get; }

        public PrimaryResponseMetadata(Type type)
        {
            ArgumentGuard.NotNull(type, nameof(type));

            Type = type;
        }
    }
}
