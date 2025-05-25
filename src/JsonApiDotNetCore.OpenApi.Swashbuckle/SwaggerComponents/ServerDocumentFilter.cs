using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.SwaggerComponents;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
internal sealed class ServerDocumentFilter : IDocumentFilter
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ServerDocumentFilter(IHttpContextAccessor httpContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(httpContextAccessor);

        _httpContextAccessor = httpContextAccessor;
    }

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        swaggerDoc.Servers ??= [];

        if (swaggerDoc.Servers.Count == 0 && _httpContextAccessor.HttpContext != null)
        {
            HttpRequest httpRequest = _httpContextAccessor.HttpContext.Request;

            swaggerDoc.Servers.Add(new OpenApiServer
            {
                Url = $"{httpRequest.Scheme}://{httpRequest.Host.Value}"
            });
        }
    }
}
