using System;
using System.Threading.Tasks;
using JsonApiDotNetCore.Serialization;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCore.Middleware
{
    /// <summary>
    /// Extensibility point for reading incoming HTTP request.
    /// </summary>
    public sealed class JsonApiInputFormatter : IInputFormatter
    {
        public bool CanRead(InputFormatterContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return context.HttpContext.IsJsonApiRequest();
        }

        public async Task<InputFormatterResult> ReadAsync(InputFormatterContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var reader = context.HttpContext.RequestServices.GetService<IJsonApiReader>();
            return await reader.ReadAsync(context);
        }
    }
}
