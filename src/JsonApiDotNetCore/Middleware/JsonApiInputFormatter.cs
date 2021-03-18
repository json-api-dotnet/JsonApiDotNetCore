using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

namespace JsonApiDotNetCore.Middleware
{
    /// <inheritdoc />
    public sealed class JsonApiInputFormatter : IJsonApiInputFormatter
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
                case HeaderConstants.AtomicOperationsMediaType when typeof(IEnumerable<OperationContainer>).IsAssignableFrom(objectType):
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
            return typeof(IEnumerable<IIdentifiable>).IsAssignableFrom(type) || type.IsOrImplementsInterface(typeof(IIdentifiable)) || type == typeof(object);
        }
    }
}
