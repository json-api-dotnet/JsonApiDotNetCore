using System;
using System.Threading.Tasks;
using JsonApiDotNetCore.Serialization;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCore.Middleware
{
    public sealed class JsonApiOutputFormatter : IOutputFormatter
    {
        public bool CanWriteResult(OutputFormatterCanWriteContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return context.HttpContext.IsJsonApiRequest();
        }

        public async Task WriteAsync(OutputFormatterWriteContext context)
        {
            var writer = context.HttpContext.RequestServices.GetService<IJsonApiWriter>();
            await writer.WriteAsync(context);
        }
    }
}
