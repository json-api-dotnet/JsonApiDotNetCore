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
        if (_httpContextAccessor.HttpContext != null)
        {
            swaggerDoc.Servers ??= new List<OpenApiServer>();

            if (swaggerDoc.Servers.Count == 0)
            {
                HttpRequest httpRequest = _httpContextAccessor.HttpContext.Request;

                swaggerDoc.Servers.Add(new OpenApiServer
                {
                    Url = $"{httpRequest.Scheme}://{httpRequest.Host.Value}"
                });
            }
        }
    }
}
