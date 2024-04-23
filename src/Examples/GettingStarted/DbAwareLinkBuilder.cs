using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Parsing;
using JsonApiDotNetCore.Serialization.Response;
using Microsoft.Extensions.Primitives;

namespace GettingStarted;

public sealed class DbAwareLinkBuilder(
    IJsonApiOptions options, IJsonApiRequest request, IPaginationContext paginationContext, IHttpContextAccessor httpContextAccessor,
    LinkGenerator linkGenerator, IControllerResourceMapping controllerResourceMapping, IPaginationParser paginationParser,
    IDocumentDescriptionLinkProvider documentDescriptionLinkProvider) : LinkBuilder(options, request, paginationContext, httpContextAccessor, linkGenerator,
    controllerResourceMapping, paginationParser, documentDescriptionLinkProvider)
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    protected override string? RenderLinkForAction(string? controllerName, string actionName, IDictionary<string, object?> routeValues)
    {
        if (!routeValues.ContainsKey("dbType"))
        {
            HttpContext? httpContext = _httpContextAccessor.HttpContext;

            if (httpContext != null)
            {
                StringValues dbType = httpContext.Request.Query["dbType"];
                routeValues.Add("dbType", dbType);
            }
        }

        return base.RenderLinkForAction(controllerName, actionName, routeValues);
    }
}
