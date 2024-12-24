using JsonApiDotNetCore.Serialization.Response;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCore.Middleware;

/// <inheritdoc cref="IJsonApiOutputFormatter" />
public sealed class JsonApiOutputFormatter : IJsonApiOutputFormatter
{
    /// <inheritdoc />
    public bool CanWriteResult(OutputFormatterCanWriteContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return context.HttpContext.IsJsonApiRequest();
    }

    /// <inheritdoc />
    public async Task WriteAsync(OutputFormatterWriteContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var writer = context.HttpContext.RequestServices.GetRequiredService<IJsonApiWriter>();
        await writer.WriteAsync(context.Object, context.HttpContext);
    }
}
