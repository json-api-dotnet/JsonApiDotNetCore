using JsonApiDotNetCore.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle;

/// <inheritdoc />
internal sealed class JsonApiRequestAccessor : IJsonApiRequestAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <inheritdoc />
    public IJsonApiRequest? Current => _httpContextAccessor.HttpContext?.RequestServices.GetService<IJsonApiRequest>();

    public JsonApiRequestAccessor(IHttpContextAccessor httpContextAccessor)
    {
        ArgumentGuard.NotNull(httpContextAccessor);

        _httpContextAccessor = httpContextAccessor;
    }
}
