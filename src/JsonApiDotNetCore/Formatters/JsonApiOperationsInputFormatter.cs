using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace JsonApiDotNetCore.Formatters
{
    public class JsonApiOperationsInputFormatter : TextInputFormatter
    {
        const string PROFILE_EXTENSION = "<http://example.org/profiles/myjsonstuff>; rel=\"profile\"";

        public JsonApiOperationsInputFormatter()
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/vnd.api+json"));
            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
        }

        public override bool CanRead(InputFormatterContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var contentTypeString = context.HttpContext.Request.ContentType;

            var canRead = (
                contentTypeString == "application/vnd.api+json" &&
                context.HttpContext.Request.Headers.TryGetValue("Link", out StringValues profileExtension) &&
                profileExtension == PROFILE_EXTENSION
            );
            Console.WriteLine($">>> JsonApiOperationsInputFormatter Can Read {canRead}");
            return canRead;
        }

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
        {
            Console.WriteLine($">>> JsonApiOperationsInputFormatter ReadAsync");
            var reader = context.HttpContext.RequestServices.GetService<IJsonApiOperationsReader>();
            return await reader.ReadAsync(context);
        }
    }
}
