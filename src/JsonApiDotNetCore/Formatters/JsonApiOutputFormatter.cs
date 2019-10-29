using System;
using System.Threading.Tasks;
using JsonApiDotNetCore.Internal;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCore.Formatters
{
    public class JsonApiOutputFormatter : IOutputFormatter
    {
        public bool CanWriteResult(OutputFormatterCanWriteContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var contentTypeString = context.HttpContext.Request.ContentType;

            return string.IsNullOrEmpty(contentTypeString) || contentTypeString == Constants.ContentType;
        }
        public async Task WriteAsync(OutputFormatterWriteContext context)
        {
            var writer = context.HttpContext.RequestServices.GetService<IJsonApiWriter>();
            await writer.WriteAsync(context);
        }
    }
}
