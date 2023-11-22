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
        ArgumentGuard.NotNull(context);

        return context.HttpContext.IsJsonApiRequest();
    }

    /// <inheritdoc />
    public Task WriteAsync(OutputFormatterWriteContext context)
    {
        ArgumentGuard.NotNull(context);

        var writer = context.HttpContext.RequestServices.GetRequiredService<IJsonApiWriter>();
        return writer.WriteAsync(context.Object, context.HttpContext);
    }
}
