using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

namespace JsonApiDotNetCore.Middleware
{
    public sealed class JsonApiOutputFormatter : IJsonApiOutputFormatter, IApiRequestFormatMetadataProvider
    {
        private static readonly Type OperationContainerType = typeof(OperationContainer);

        /// <inheritdoc />
        public bool CanWriteResult(OutputFormatterCanWriteContext context)
        {
            ArgumentGuard.NotNull(context, nameof(context));

            return context.HttpContext.IsJsonApiRequest();
        }

        /// <inheritdoc />
        public async Task WriteAsync(OutputFormatterWriteContext context)
        {
            ArgumentGuard.NotNull(context, nameof(context));

            var writer = context.HttpContext.RequestServices.GetRequiredService<IJsonApiWriter>();
            await writer.WriteAsync(context);
        }

        /// <inheritdoc />
        public IReadOnlyList<string> GetSupportedContentTypes(string contentType, Type objectType)
        {
            ArgumentGuard.NotNull(contentType, nameof(contentType));
            ArgumentGuard.NotNull(objectType, nameof(objectType));

            string mediaType = IsAtomicOperationsType(objectType) ? HeaderConstants.AtomicOperationsMediaType : HeaderConstants.MediaType;

            return new MediaTypeCollection
            {
                new MediaTypeHeaderValue(mediaType)
            };
        }

        private bool IsAtomicOperationsType(Type objectType)
        {
            return objectType.GetInterface(nameof(IEnumerable)) != null && objectType.GetGenericArguments().First() == OperationContainerType;
        }
    }
}
