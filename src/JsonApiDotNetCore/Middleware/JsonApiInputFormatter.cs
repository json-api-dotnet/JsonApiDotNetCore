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
    public sealed class JsonApiInputFormatter : IJsonApiInputFormatter, IApiRequestFormatMetadataProvider
    {
        /// <inheritdoc />
        public bool CanRead(InputFormatterContext context)
        {
            ArgumentGuard.NotNull(context, nameof(context));

            return context.HttpContext.IsJsonApiRequest();
        }

        /// <inheritdoc />
        public async Task<InputFormatterResult> ReadAsync(InputFormatterContext context)
        {
            ArgumentGuard.NotNull(context, nameof(context));

            var reader = context.HttpContext.RequestServices.GetRequiredService<IJsonApiReader>();
            return await reader.ReadAsync(context);
        }

        /// <inheritdoc />
        public IReadOnlyList<string> GetSupportedContentTypes(string contentType, Type objectType)
        {
            ArgumentGuard.NotNull(objectType, nameof(objectType));

            var mediaTypes = new MediaTypeCollection();

            switch (contentType)
            {
                case HeaderConstants.AtomicOperationsMediaType when IsOperationsType(objectType):
                {
                    mediaTypes.Add(MediaTypeHeaderValue.Parse(HeaderConstants.AtomicOperationsMediaType));
                    break;
                }
                case HeaderConstants.MediaType when IsJsonApiResource(objectType):
                {
                    mediaTypes.Add(MediaTypeHeaderValue.Parse(HeaderConstants.MediaType));
                    break;
                }
            }

            return mediaTypes;
        }

        private bool IsJsonApiResource(Type type)
        {
            Type typeToCheck = typeof(IEnumerable).IsAssignableFrom(type) ? type.GetGenericArguments()[0] : type;

            return typeToCheck.IsOrImplementsInterface(typeof(IIdentifiable)) || typeToCheck == typeof(object);
        }

        private bool IsOperationsType(Type type)
        {
            Type typeToCheck = typeof(IEnumerable).IsAssignableFrom(type) ? type.GetGenericArguments()[0] : type;

            return typeToCheck == typeof(OperationContainer);
        }
    }
}
