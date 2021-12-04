using System.Collections.Immutable;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.Internal.Parsing;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Routing;

namespace JsonApiDotNetCore.Serialization.Response;

[PublicAPI]
public class LinkBuilder : ILinkBuilder
{
    private const string PageSizeParameterName = "page[size]";
    private const string PageNumberParameterName = "page[number]";

    private static readonly string GetPrimaryControllerActionName = NoAsyncSuffix(nameof(BaseJsonApiController<Identifiable<int>, int>.GetAsync));
    private static readonly string GetSecondaryControllerActionName = NoAsyncSuffix(nameof(BaseJsonApiController<Identifiable<int>, int>.GetSecondaryAsync));

    private static readonly string GetRelationshipControllerActionName =
        NoAsyncSuffix(nameof(BaseJsonApiController<Identifiable<int>, int>.GetRelationshipAsync));

    private readonly IJsonApiOptions _options;
    private readonly IJsonApiRequest _request;
    private readonly IPaginationContext _paginationContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly LinkGenerator _linkGenerator;
    private readonly IControllerResourceMapping _controllerResourceMapping;

    private HttpContext HttpContext
    {
        get
        {
            if (_httpContextAccessor.HttpContext == null)
            {
                throw new InvalidOperationException("An active HTTP request is required.");
            }

            return _httpContextAccessor.HttpContext;
        }
    }

    public LinkBuilder(IJsonApiOptions options, IJsonApiRequest request, IPaginationContext paginationContext, IHttpContextAccessor httpContextAccessor,
        LinkGenerator linkGenerator, IControllerResourceMapping controllerResourceMapping)
    {
        ArgumentGuard.NotNull(options, nameof(options));
        ArgumentGuard.NotNull(request, nameof(request));
        ArgumentGuard.NotNull(paginationContext, nameof(paginationContext));
        ArgumentGuard.NotNull(linkGenerator, nameof(linkGenerator));
        ArgumentGuard.NotNull(controllerResourceMapping, nameof(controllerResourceMapping));

        _options = options;
        _request = request;
        _paginationContext = paginationContext;
        _httpContextAccessor = httpContextAccessor;
        _linkGenerator = linkGenerator;
        _controllerResourceMapping = controllerResourceMapping;
    }

    private static string NoAsyncSuffix(string actionName)
    {
        return actionName.EndsWith("Async", StringComparison.Ordinal) ? actionName[..^"Async".Length] : actionName;
    }

    /// <inheritdoc />
    public TopLevelLinks? GetTopLevelLinks()
    {
        var links = new TopLevelLinks();
        ResourceType? resourceType = _request.SecondaryResourceType ?? _request.PrimaryResourceType;

        if (ShouldIncludeTopLevelLink(LinkTypes.Self, resourceType))
        {
            links.Self = GetLinkForTopLevelSelf();
        }

        if (_request.Kind == EndpointKind.Relationship && _request.Relationship != null && ShouldIncludeTopLevelLink(LinkTypes.Related, resourceType))
        {
            links.Related = GetLinkForRelationshipRelated(_request.PrimaryId!, _request.Relationship);
        }

        if (_request.IsCollection && _paginationContext.PageSize != null && ShouldIncludeTopLevelLink(LinkTypes.Paging, resourceType))
        {
            SetPaginationInTopLevelLinks(resourceType!, links);
        }

        return links.HasValue() ? links : null;
    }

    /// <summary>
    /// Checks if the top-level <paramref name="linkType" /> should be added by first checking configuration on the <see cref="ResourceType" />, and if not
    /// configured, by checking with the global configuration in <see cref="IJsonApiOptions" />.
    /// </summary>
    private bool ShouldIncludeTopLevelLink(LinkTypes linkType, ResourceType? resourceType)
    {
        if (resourceType != null && resourceType.TopLevelLinks != LinkTypes.NotConfigured)
        {
            return resourceType.TopLevelLinks.HasFlag(linkType);
        }

        return _options.TopLevelLinks.HasFlag(linkType);
    }

    private string GetLinkForTopLevelSelf()
    {
        // Note: in tests, this does not properly escape special characters due to WebApplicationFactory short-circuiting.
        return _options.UseRelativeLinks ? HttpContext.Request.GetEncodedPathAndQuery() : HttpContext.Request.GetEncodedUrl();
    }

    private void SetPaginationInTopLevelLinks(ResourceType resourceType, TopLevelLinks links)
    {
        string? pageSizeValue = CalculatePageSizeValue(_paginationContext.PageSize, resourceType);

        links.First = GetLinkForPagination(1, pageSizeValue);

        if (_paginationContext.TotalPageCount > 0)
        {
            links.Last = GetLinkForPagination(_paginationContext.TotalPageCount.Value, pageSizeValue);
        }

        if (_paginationContext.PageNumber.OneBasedValue > 1)
        {
            links.Prev = GetLinkForPagination(_paginationContext.PageNumber.OneBasedValue - 1, pageSizeValue);
        }

        bool hasNextPage = _paginationContext.PageNumber.OneBasedValue < _paginationContext.TotalPageCount;
        bool possiblyHasNextPage = _paginationContext.TotalPageCount == null && _paginationContext.IsPageFull;

        if (hasNextPage || possiblyHasNextPage)
        {
            links.Next = GetLinkForPagination(_paginationContext.PageNumber.OneBasedValue + 1, pageSizeValue);
        }
    }

    private string? CalculatePageSizeValue(PageSize? topPageSize, ResourceType resourceType)
    {
        string pageSizeParameterValue = HttpContext.Request.Query[PageSizeParameterName];

        PageSize? newTopPageSize = Equals(topPageSize, _options.DefaultPageSize) ? null : topPageSize;
        return ChangeTopPageSize(pageSizeParameterValue, newTopPageSize, resourceType);
    }

    private string? ChangeTopPageSize(string pageSizeParameterValue, PageSize? topPageSize, ResourceType resourceType)
    {
        IImmutableList<PaginationElementQueryStringValueExpression> elements = ParsePageSizeExpression(pageSizeParameterValue, resourceType);
        int elementInTopScopeIndex = elements.FindIndex(expression => expression.Scope == null);

        if (topPageSize != null)
        {
            var topPageSizeElement = new PaginationElementQueryStringValueExpression(null, topPageSize.Value);

            elements = elementInTopScopeIndex != -1 ? elements.SetItem(elementInTopScopeIndex, topPageSizeElement) : elements.Insert(0, topPageSizeElement);
        }
        else
        {
            if (elementInTopScopeIndex != -1)
            {
                elements = elements.RemoveAt(elementInTopScopeIndex);
            }
        }

        string parameterValue = string.Join(',',
            elements.Select(expression => expression.Scope == null ? expression.Value.ToString() : $"{expression.Scope}:{expression.Value}"));

        return parameterValue == string.Empty ? null : parameterValue;
    }

    private IImmutableList<PaginationElementQueryStringValueExpression> ParsePageSizeExpression(string? pageSizeParameterValue, ResourceType resourceType)
    {
        if (pageSizeParameterValue == null)
        {
            return ImmutableArray<PaginationElementQueryStringValueExpression>.Empty;
        }

        var parser = new PaginationParser();
        PaginationQueryStringValueExpression paginationExpression = parser.Parse(pageSizeParameterValue, resourceType);

        return paginationExpression.Elements;
    }

    private string GetLinkForPagination(int pageOffset, string? pageSizeValue)
    {
        string queryStringValue = GetQueryStringInPaginationLink(pageOffset, pageSizeValue);

        var builder = new UriBuilder(HttpContext.Request.GetEncodedUrl())
        {
            Query = queryStringValue
        };

        UriComponents components = _options.UseRelativeLinks ? UriComponents.PathAndQuery : UriComponents.AbsoluteUri;
        return builder.Uri.GetComponents(components, UriFormat.SafeUnescaped);
    }

    private string GetQueryStringInPaginationLink(int pageOffset, string? pageSizeValue)
    {
        IDictionary<string, string?> parameters = HttpContext.Request.Query.ToDictionary(pair => pair.Key, pair => (string?)pair.Value.ToString());

        if (pageSizeValue == null)
        {
            parameters.Remove(PageSizeParameterName);
        }
        else
        {
            parameters[PageSizeParameterName] = pageSizeValue;
        }

        if (pageOffset == 1)
        {
            parameters.Remove(PageNumberParameterName);
        }
        else
        {
            parameters[PageNumberParameterName] = pageOffset.ToString();
        }

        return QueryString.Create(parameters).Value ?? string.Empty;
    }

    /// <inheritdoc />
    public ResourceLinks? GetResourceLinks(ResourceType resourceType, IIdentifiable resource)
    {
        ArgumentGuard.NotNull(resourceType, nameof(resourceType));
        ArgumentGuard.NotNull(resource, nameof(resource));

        var links = new ResourceLinks();

        if (ShouldIncludeResourceLink(LinkTypes.Self, resourceType))
        {
            links.Self = GetLinkForResourceSelf(resourceType, resource);
        }

        return links.HasValue() ? links : null;
    }

    /// <summary>
    /// Checks if the resource object level <paramref name="linkType" /> should be added by first checking configuration on the <see cref="ResourceType" />,
    /// and if not configured, by checking with the global configuration in <see cref="IJsonApiOptions" />.
    /// </summary>
    private bool ShouldIncludeResourceLink(LinkTypes linkType, ResourceType resourceType)
    {
        if (resourceType.ResourceLinks != LinkTypes.NotConfigured)
        {
            return resourceType.ResourceLinks.HasFlag(linkType);
        }

        return _options.ResourceLinks.HasFlag(linkType);
    }

    private string? GetLinkForResourceSelf(ResourceType resourceType, IIdentifiable resource)
    {
        string? controllerName = _controllerResourceMapping.GetControllerNameForResourceType(resourceType);
        IDictionary<string, object?> routeValues = GetRouteValues(resource.StringId!, null);

        return RenderLinkForAction(controllerName, GetPrimaryControllerActionName, routeValues);
    }

    /// <inheritdoc />
    public RelationshipLinks? GetRelationshipLinks(RelationshipAttribute relationship, IIdentifiable leftResource)
    {
        ArgumentGuard.NotNull(relationship, nameof(relationship));
        ArgumentGuard.NotNull(leftResource, nameof(leftResource));

        var links = new RelationshipLinks();

        if (ShouldIncludeRelationshipLink(LinkTypes.Self, relationship))
        {
            links.Self = GetLinkForRelationshipSelf(leftResource.StringId!, relationship);
        }

        if (ShouldIncludeRelationshipLink(LinkTypes.Related, relationship))
        {
            links.Related = GetLinkForRelationshipRelated(leftResource.StringId!, relationship);
        }

        return links.HasValue() ? links : null;
    }

    private string? GetLinkForRelationshipSelf(string leftId, RelationshipAttribute relationship)
    {
        string? controllerName = _controllerResourceMapping.GetControllerNameForResourceType(relationship.LeftType);
        IDictionary<string, object?> routeValues = GetRouteValues(leftId, relationship.PublicName);

        return RenderLinkForAction(controllerName, GetRelationshipControllerActionName, routeValues);
    }

    private string? GetLinkForRelationshipRelated(string leftId, RelationshipAttribute relationship)
    {
        string? controllerName = _controllerResourceMapping.GetControllerNameForResourceType(relationship.LeftType);
        IDictionary<string, object?> routeValues = GetRouteValues(leftId, relationship.PublicName);

        return RenderLinkForAction(controllerName, GetSecondaryControllerActionName, routeValues);
    }

    private IDictionary<string, object?> GetRouteValues(string primaryId, string? relationshipName)
    {
        // By default, we copy all route parameters from the *current* endpoint, which helps in case all endpoints have the same
        // set of non-standard parameters. There is no way we can know which non-standard parameters a *different* endpoint needs,
        // so users must override RenderLinkForAction to supply them, if applicable.
        RouteValueDictionary routeValues = HttpContext.Request.RouteValues;

        routeValues["id"] = primaryId;
        routeValues["relationshipName"] = relationshipName;

        return routeValues;
    }

    protected virtual string? RenderLinkForAction(string? controllerName, string actionName, IDictionary<string, object?> routeValues)
    {
        return _options.UseRelativeLinks
            ? _linkGenerator.GetPathByAction(HttpContext, actionName, controllerName, routeValues)
            : _linkGenerator.GetUriByAction(HttpContext, actionName, controllerName, routeValues);
    }

    /// <summary>
    /// Checks if the relationship object level <paramref name="linkType" /> should be added by first checking configuration on the
    /// <paramref name="relationship" /> attribute, if not configured by checking <see cref="ResourceLinksAttribute.RelationshipLinks" /> on the resource
    /// type that contains this relationship, and if not configured by checking with the global configuration in <see cref="IJsonApiOptions" />.
    /// </summary>
    private bool ShouldIncludeRelationshipLink(LinkTypes linkType, RelationshipAttribute relationship)
    {
        if (relationship.Links != LinkTypes.NotConfigured)
        {
            return relationship.Links.HasFlag(linkType);
        }

        if (relationship.LeftType.RelationshipLinks != LinkTypes.NotConfigured)
        {
            return relationship.LeftType.RelationshipLinks.HasFlag(linkType);
        }

        return _options.RelationshipLinks.HasFlag(linkType);
    }
}
