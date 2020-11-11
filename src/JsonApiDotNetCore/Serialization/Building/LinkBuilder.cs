using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.Internal.Parsing;
using JsonApiDotNetCore.QueryStrings;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCore.Serialization.Building
{
    public class LinkBuilder : ILinkBuilder
    {
        private const string _pageSizeParameterName = "page[size]";
        private const string _pageNumberParameterName = "page[number]";

        private readonly IResourceContextProvider _provider;
        private readonly IRequestQueryStringAccessor _queryStringAccessor;
        private readonly IJsonApiOptions _options;
        private readonly IJsonApiRequest _request;
        private readonly IPaginationContext _paginationContext;

        public LinkBuilder(IJsonApiOptions options,
                           IJsonApiRequest request,
                           IPaginationContext paginationContext,
                           IResourceContextProvider provider,
                           IRequestQueryStringAccessor queryStringAccessor)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _request = request ?? throw new ArgumentNullException(nameof(request));
            _paginationContext = paginationContext ?? throw new ArgumentNullException(nameof(paginationContext));
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _queryStringAccessor = queryStringAccessor ?? throw new ArgumentNullException(nameof(queryStringAccessor));
        }

        /// <inheritdoc />
        public TopLevelLinks GetTopLevelLinks()
        {
            ResourceContext resourceContext = _request.PrimaryResource;

            TopLevelLinks topLevelLinks = null;
            if (ShouldAddTopLevelLink(resourceContext, LinkTypes.Self))
            {
                topLevelLinks = new TopLevelLinks {Self = GetSelfTopLevelLink(resourceContext, null)};
            }

            if (ShouldAddTopLevelLink(resourceContext, LinkTypes.Related) && _request.Kind == EndpointKind.Relationship)
            {   
                topLevelLinks ??= new TopLevelLinks();
                topLevelLinks.Related = GetRelatedRelationshipLink(_request.PrimaryResource.PublicName, _request.PrimaryId, _request.Relationship.PublicName);
            }

            if (ShouldAddTopLevelLink(resourceContext, LinkTypes.Paging) && _paginationContext.PageSize != null && _request.IsCollection)
            {   
                SetPageLinks(resourceContext, topLevelLinks ??= new TopLevelLinks());
            }

            return topLevelLinks;
        }

        /// <summary>
        /// Checks if the top-level <paramref name="link"/> should be added by first checking
        /// configuration on the <see cref="ResourceContext"/>, and if not configured, by checking with the
        /// global configuration in <see cref="IJsonApiOptions"/>.
        /// </summary>
        private bool ShouldAddTopLevelLink(ResourceContext resourceContext, LinkTypes link)
        {
            if (resourceContext.TopLevelLinks != LinkTypes.NotConfigured)
            {
                return resourceContext.TopLevelLinks.HasFlag(link);
            }

            return _options.TopLevelLinks.HasFlag(link);
        }

        private void SetPageLinks(ResourceContext resourceContext, TopLevelLinks links)
        {
            links.First = GetPageLink(resourceContext, 1, _paginationContext.PageSize);

            if (_paginationContext.TotalPageCount > 0)
            {
                links.Last = GetPageLink(resourceContext, _paginationContext.TotalPageCount.Value, _paginationContext.PageSize);
            }

            if (_paginationContext.PageNumber.OneBasedValue > 1)
            {
                links.Prev = GetPageLink(resourceContext, _paginationContext.PageNumber.OneBasedValue - 1, _paginationContext.PageSize);
            }

            bool hasNextPage = _paginationContext.PageNumber.OneBasedValue < _paginationContext.TotalPageCount;
            bool possiblyHasNextPage = _paginationContext.TotalPageCount == null && _paginationContext.IsPageFull;

            if (hasNextPage || possiblyHasNextPage)
            {
                links.Next = GetPageLink(resourceContext, _paginationContext.PageNumber.OneBasedValue + 1, _paginationContext.PageSize);
            }
        }

        private string GetSelfTopLevelLink(ResourceContext resourceContext, Action<Dictionary<string, string>> queryStringUpdateAction)
        {
            var builder = new StringBuilder();
            builder.Append(_request.BasePath);
            builder.Append("/");
            builder.Append(resourceContext.PublicName);

            if (_request.PrimaryId != null)
            {
                builder.Append("/");
                builder.Append(_request.PrimaryId);
            }

            if (_request.Kind == EndpointKind.Relationship)
            {
                builder.Append("/relationships");
            }

            if (_request.Relationship != null)
            {
                builder.Append("/");
                builder.Append(_request.Relationship.PublicName);
            }

            string queryString = BuildQueryString(queryStringUpdateAction);
            builder.Append(queryString);

            return builder.ToString();
        }

        private string BuildQueryString(Action<Dictionary<string, string>> updateAction)
        {
            var parameters = _queryStringAccessor.Query.ToDictionary(pair => pair.Key, pair => pair.Value.ToString());
            updateAction?.Invoke(parameters);
            string queryString = QueryString.Create(parameters).Value;

            return DecodeSpecialCharacters(queryString);
        }

        private static string DecodeSpecialCharacters(string uri)
        {
            return uri.Replace("%5B", "[").Replace("%5D", "]").Replace("%27", "'").Replace("%3A", ":");
        }

        private string GetPageLink(ResourceContext resourceContext, int pageOffset, PageSize pageSize)
        {
            return GetSelfTopLevelLink(resourceContext, parameters =>
            {
                var existingPageSizeParameterValue = parameters.ContainsKey(_pageSizeParameterName)
                    ? parameters[_pageSizeParameterName]
                    : null;

                PageSize newTopPageSize = Equals(pageSize, _options.DefaultPageSize) ? null : pageSize;

                string newPageSizeParameterValue = ChangeTopPageSize(existingPageSizeParameterValue, newTopPageSize);
                if (newPageSizeParameterValue == null)
                {
                    parameters.Remove(_pageSizeParameterName);
                }
                else
                {
                    parameters[_pageSizeParameterName] = newPageSizeParameterValue;
                }

                if (pageOffset == 1)
                {
                    parameters.Remove(_pageNumberParameterName);
                }
                else
                {
                    parameters[_pageNumberParameterName] = pageOffset.ToString();
                }
            });
        }

        private string ChangeTopPageSize(string pageSizeParameterValue, PageSize topPageSize)
        {
            var elements = ParsePageSizeExpression(pageSizeParameterValue);
            var elementInTopScopeIndex = elements.FindIndex(expression => expression.Scope == null);

            if (topPageSize != null)
            {
                var topPageSizeElement = new PaginationElementQueryStringValueExpression(null, topPageSize.Value);

                if (elementInTopScopeIndex != -1)
                {
                    elements[elementInTopScopeIndex] = topPageSizeElement;
                }
                else
                {
                    elements.Insert(0, topPageSizeElement);
                }
            }
            else
            {
                if (elementInTopScopeIndex != -1)
                {
                    elements.RemoveAt(elementInTopScopeIndex);
                }
            }

            var parameterValue = string.Join(',',
                elements.Select(expression => expression.Scope == null ? expression.Value.ToString() : $"{expression.Scope}:{expression.Value}"));

            return parameterValue == string.Empty ? null : parameterValue;
        }

        private List<PaginationElementQueryStringValueExpression> ParsePageSizeExpression(string pageSizeParameterValue)
        {
            if (pageSizeParameterValue == null)
            {
                return new List<PaginationElementQueryStringValueExpression>();
            }

            var requestResource = _request.SecondaryResource ?? _request.PrimaryResource;

            var parser = new PaginationParser(_provider);
            var paginationExpression = parser.Parse(pageSizeParameterValue, requestResource);

            return new List<PaginationElementQueryStringValueExpression>(paginationExpression.Elements);
        }

        /// <inheritdoc />
        public ResourceLinks GetResourceLinks(string resourceName, string id)
        {
            if (resourceName == null) throw new ArgumentNullException(nameof(resourceName));
            if (id == null) throw new ArgumentNullException(nameof(id));

            var resourceContext = _provider.GetResourceContext(resourceName);
            if (ShouldAddResourceLink(resourceContext, LinkTypes.Self))
            {
                return new ResourceLinks { Self = GetSelfResourceLink(resourceName, id) };
            }

            return null;
        }

        /// <inheritdoc />
        public RelationshipLinks GetRelationshipLinks(RelationshipAttribute relationship, IIdentifiable parent)
        {
            if (relationship == null) throw new ArgumentNullException(nameof(relationship));
            if (parent == null) throw new ArgumentNullException(nameof(parent));

            var parentResourceContext = _provider.GetResourceContext(parent.GetType());
            var childNavigation = relationship.PublicName;
            RelationshipLinks links = null;
            if (ShouldAddRelationshipLink(parentResourceContext, relationship, LinkTypes.Related))
            {
                links = new RelationshipLinks { Related = GetRelatedRelationshipLink(parentResourceContext.PublicName, parent.StringId, childNavigation) };
            }

            if (ShouldAddRelationshipLink(parentResourceContext, relationship, LinkTypes.Self))
            {
                links ??= new RelationshipLinks();
                links.Self = GetSelfRelationshipLink(parentResourceContext.PublicName, parent.StringId, childNavigation);
            }

            return links;
        }


        private string GetSelfRelationshipLink(string parent, string parentId, string navigation)
        {
            return $"{_request.BasePath}/{parent}/{parentId}/relationships/{navigation}";
        }

        private string GetSelfResourceLink(string resource, string resourceId)
        {
            return $"{_request.BasePath}/{resource}/{resourceId}";
        }

        private string GetRelatedRelationshipLink(string parent, string parentId, string navigation)
        {
            return $"{_request.BasePath}/{parent}/{parentId}/{navigation}";
        }

        /// <summary>
        /// Checks if the resource object level <paramref name="link"/> should be added by first checking
        /// configuration on the <see cref="ResourceContext"/>, and if not configured, by checking with the
        /// global configuration in <see cref="IJsonApiOptions"/>.
        /// </summary>
        private bool ShouldAddResourceLink(ResourceContext resourceContext, LinkTypes link)
        {
            if (_request.Kind == EndpointKind.Relationship)
            {
                return false;
            }

            if (resourceContext.ResourceLinks != LinkTypes.NotConfigured)
            {
                return resourceContext.ResourceLinks.HasFlag(link);
            }
            return _options.ResourceLinks.HasFlag(link);
        }

        /// <summary>
        /// Checks if the resource object level <paramref name="link"/> should be added by first checking
        /// configuration on the <paramref name="relationship"/> attribute, if not configured by checking
        /// the <see cref="ResourceContext"/>, and if not configured by checking with the
        /// global configuration in <see cref="IJsonApiOptions"/>.
        /// </summary>
        private bool ShouldAddRelationshipLink(ResourceContext resourceContext, RelationshipAttribute relationship, LinkTypes link)
        {
            if (relationship.Links != LinkTypes.NotConfigured)
            {
                return relationship.Links.HasFlag(link);
            }
            if (resourceContext.RelationshipLinks != LinkTypes.NotConfigured)
            {
                return resourceContext.RelationshipLinks.HasFlag(link);
            }

            return _options.RelationshipLinks.HasFlag(link);
        }
    }
}
