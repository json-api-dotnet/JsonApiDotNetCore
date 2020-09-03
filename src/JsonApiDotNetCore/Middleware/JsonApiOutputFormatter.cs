using System;
using System.Threading.Tasks;
using JsonApiDotNetCore.Serialization;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCore.Middleware
{
    /// <inheritdoc />
    public sealed class JsonApiOutputFormatter : IJsonApiOutputFormatter
    {
        /// <inheritdoc />
        public bool CanWriteResult(OutputFormatterCanWriteContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return context.HttpContext.IsJsonApiRequest();
        }

        /// <inheritdoc />
        public async Task WriteAsync(OutputFormatterWriteContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var writer = context.HttpContext.RequestServices.GetService<IJsonApiWriter>();
            await writer.WriteAsync(context);
        }
    }
}
