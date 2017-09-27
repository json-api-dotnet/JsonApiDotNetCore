using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;

namespace JsonApiDotNetCore.Formatters
{
    public class JsonApiOperationsInputFormatter : IInputFormatter
    {
        internal const string PROFILE_EXTENSION = "<http://example.org/profiles/myjsonstuff>; rel=\"profile\"";

        public bool CanRead(InputFormatterContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var contentTypeString = context.HttpContext.Request.ContentType;

            var canRead = (
                contentTypeString == "application/vnd.api+json" &&
                context.HttpContext.Request.Headers.TryGetValue("Link", out StringValues profileExtension) &&
                profileExtension == PROFILE_EXTENSION
            );

            return canRead;
        }

        public async Task<InputFormatterResult> ReadAsync(InputFormatterContext context)
        {
            var reader = context.HttpContext.RequestServices.GetService<IJsonApiOperationsReader>();
            return await reader.ReadAsync(context);
        }
    }
}
