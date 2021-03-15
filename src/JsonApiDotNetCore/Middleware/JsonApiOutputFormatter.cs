using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

namespace JsonApiDotNetCore.Middleware
{
    public sealed class JsonApiOutputFormatter : IJsonApiOutputFormatter, IApiResponseTypeMetadataProvider
    {
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
            ArgumentGuard.NotNull(objectType, nameof(objectType));

            var mediaTypes = new MediaTypeCollection();

            if (contentType == HeaderConstants.MediaType)
            {
                Type typeToCheck = typeof(IEnumerable).IsAssignableFrom(objectType) ? objectType.GetGenericArguments()[0] : objectType;

                if (typeToCheck.IsOrImplementsInterface(typeof(IIdentifiable)) || typeToCheck == typeof(object))
                {
                    mediaTypes.Add(MediaTypeHeaderValue.Parse(contentType));
                }
            }

            return mediaTypes;
        }
    }
}
