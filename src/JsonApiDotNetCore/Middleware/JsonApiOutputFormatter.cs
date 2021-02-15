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
    }
}
