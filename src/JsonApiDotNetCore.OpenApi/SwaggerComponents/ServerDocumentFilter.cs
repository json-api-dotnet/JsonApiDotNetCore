using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.SwaggerComponents;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
internal sealed class ServerDocumentFilter : IDocumentFilter
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ServerDocumentFilter(IHttpContextAccessor httpContextAccessor)
    {
        ArgumentGuard.NotNull(httpContextAccessor);

        _httpContextAccessor = httpContextAccessor;
    }

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
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
