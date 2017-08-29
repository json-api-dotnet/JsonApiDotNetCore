using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCore.Formatters
{
    public class JsonApiInputFormatter : IInputFormatter
    {
        public bool CanRead(InputFormatterContext context)
        {            
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var contentTypeString = context.HttpContext.Request.ContentType;

            var canRead = contentTypeString == "application/vnd.api+json";

            Console.WriteLine($">>> JsonApiInputFormatter Can Read {canRead}");

            return canRead;
        }

        public async Task<InputFormatterResult> ReadAsync(InputFormatterContext context)
        {
            Console.WriteLine($">>> JsonApiInputFormatter ReadAsync");
            var reader = context.HttpContext.RequestServices.GetService<IJsonApiReader>();
            return await reader.ReadAsync(context);
        }
    }
}
