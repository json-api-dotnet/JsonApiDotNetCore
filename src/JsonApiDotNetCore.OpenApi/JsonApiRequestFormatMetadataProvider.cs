using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.Documents;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.RelationshipData;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace JsonApiDotNetCore.OpenApi
{
    internal sealed class JsonApiRequestFormatMetadataProvider : IInputFormatter, IApiRequestFormatMetadataProvider
    {
        private static readonly Type[] JsonApiRequestObjectOpenType =
        {
            typeof(ToManyRelationshipRequestData<>),
            typeof(ToOneRelationshipRequestData<>),
            typeof(NullableToOneRelationshipRequestData<>),
            typeof(ResourcePostRequestDocument<>),
            typeof(ResourcePatchRequestDocument<>)
        };

        /// <inheritdoc />
        public bool CanRead(InputFormatterContext context)
        {
            return false;
        }

        /// <inheritdoc />
        public Task<InputFormatterResult> ReadAsync(InputFormatterContext context)
        {
            throw new UnreachableCodeException();
        }

        /// <inheritdoc />
        public IReadOnlyList<string> GetSupportedContentTypes(string contentType, Type objectType)
        {
            ArgumentGuard.NotNullNorEmpty(contentType, nameof(contentType));
            ArgumentGuard.NotNull(objectType, nameof(objectType));

            if (contentType == HeaderConstants.MediaType && objectType.IsGenericType &&
                JsonApiRequestObjectOpenType.Contains(objectType.GetGenericTypeDefinition()))
            {
                return new MediaTypeCollection
                {
                    new MediaTypeHeaderValue(HeaderConstants.MediaType)
                };
            }

            return new MediaTypeCollection();
        }
    }
}
