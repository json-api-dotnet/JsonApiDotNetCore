using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Serialization;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

namespace JsonApiDotNetCore.OpenApi
{
    internal sealed class JsonApiInputFormatterWithMetadata : IJsonApiInputFormatter, IApiRequestFormatMetadataProvider
    {
        public bool CanRead(InputFormatterContext context)
        {
            ArgumentGuard.NotNull(context, nameof(context));

            return context.HttpContext.IsJsonApiRequest();
        }

        public async Task<InputFormatterResult> ReadAsync(InputFormatterContext context)
        {
            ArgumentGuard.NotNull(context, nameof(context));

            var reader = context.HttpContext.RequestServices.GetRequiredService<IJsonApiReader>();
            return await reader.ReadAsync(context);
        }

        public IReadOnlyList<string> GetSupportedContentTypes(string contentType, Type objectType)
        {
            ArgumentGuard.NotNullNorEmpty(contentType, nameof(contentType));
            ArgumentGuard.NotNull(objectType, nameof(objectType));

            return new MediaTypeCollection
            {
                new MediaTypeHeaderValue(HeaderConstants.MediaType)
            };
        }
    }
}
