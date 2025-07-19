using JetBrains.Annotations;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCoreExample;

/// <summary>
/// This is normally not needed. It ensures the server URL is added to the OpenAPI file during build.
/// </summary>
[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
internal sealed class SetOpenApiServerAtBuildTimeFilter(IHttpContextAccessor httpContextAccessor) : IDocumentFilter
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            swaggerDoc.Servers.Add(new OpenApiServer
            {
                Url = "https://localhost:44340"
            });
        }
    }
}
