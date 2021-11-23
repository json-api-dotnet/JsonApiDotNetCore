using System;

namespace JsonApiDotNetCore.OpenApi.JsonApiMetadata
{
    internal sealed class PrimaryResponseMetadata : IJsonApiResponseMetadata
    {
        public Type DocumentType { get; }

        public PrimaryResponseMetadata(Type documentType)
        {
            ArgumentGuard.NotNull(documentType, nameof(documentType));

            DocumentType = documentType;
        }
    }
}
