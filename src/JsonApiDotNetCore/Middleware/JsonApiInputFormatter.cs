using System.Threading.Tasks;
using JsonApiDotNetCore.Serialization;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;

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
    }
}
