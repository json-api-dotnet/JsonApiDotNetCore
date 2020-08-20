using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.QueryStrings;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCore.Serialization.Building
{
    public class LinkBuilder : ILinkBuilder
    {
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

        /// <inheritdoc/>
        public TopLevelLinks GetTopLevelLinks()
        {
            ResourceContext resourceContext = _request.PrimaryResource;

            TopLevelLinks topLevelLinks = null;
            if (ShouldAddTopLevelLink(resourceContext, Links.Self))
            {
                topLevelLinks = new TopLevelLinks { Self = GetSelfTopLevelLink(resourceContext) };
            }

            if (ShouldAddTopLevelLink(resourceContext, Links.Paging) && _paginationContext.PageSize != null)
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
        private bool ShouldAddTopLevelLink(ResourceContext resourceContext, Links link)
        {
            if (resourceContext.TopLevelLinks != Links.NotConfigured)
            {
                return resourceContext.TopLevelLinks.HasFlag(link);
            }

            return _options.TopLevelLinks.HasFlag(link);
        }

        private void SetPageLinks(ResourceContext resourceContext, TopLevelLinks links)
        {
            if (_paginationContext.PageNumber.OneBasedValue > 1)
            {
                links.Prev = GetPageLink(resourceContext, _paginationContext.PageNumber.OneBasedValue - 1, _paginationContext.PageSize);
            }

            if (_paginationContext.PageNumber.OneBasedValue < _paginationContext.TotalPageCount)
            {
                links.Next = GetPageLink(resourceContext, _paginationContext.PageNumber.OneBasedValue + 1, _paginationContext.PageSize);
            }

            if (_paginationContext.TotalPageCount > 0)
            {
                links.Self = GetPageLink(resourceContext, _paginationContext.PageNumber.OneBasedValue, _paginationContext.PageSize);
                links.First = GetPageLink(resourceContext, 1, _paginationContext.PageSize);
                links.Last = GetPageLink(resourceContext, _paginationContext.TotalPageCount.Value, _paginationContext.PageSize);
            }
        }

        private string GetSelfTopLevelLink(ResourceContext resourceContext)
        {
            var builder = new StringBuilder();
            builder.Append(_request.BasePath);
            builder.Append("/");
            builder.Append(resourceContext.ResourceName);

            string resourceId = _request.PrimaryId;
            if (resourceId != null)
            {
                builder.Append("/");
                builder.Append(resourceId);
            }

            if (_request.Relationship != null)
            {
                builder.Append("/");
                builder.Append(_request.Relationship.PublicName);
            }

            builder.Append(DecodeSpecialCharacters(_queryStringAccessor.QueryString.Value));

            return builder.ToString();
        }

        private string GetPageLink(ResourceContext resourceContext, int pageOffset, PageSize pageSize)
        {
            string queryString = BuildQueryString(parameters =>
            {
                parameters["page[size]"] = pageSize.ToString();
                parameters["page[number]"] = pageOffset.ToString();
            });

            return $"{_request.BasePath}/{resourceContext.ResourceName}" + queryString;
        }

        private string BuildQueryString(Action<Dictionary<string, string>> updateAction)
        {
            var parameters = _queryStringAccessor.Query.ToDictionary(pair => pair.Key, pair => pair.Value.ToString());
            updateAction(parameters);
            string queryString = QueryString.Create(parameters).Value;

            return DecodeSpecialCharacters(queryString);
        }

        private static string DecodeSpecialCharacters(string uri)
        {
            return uri.Replace("%5B", "[").Replace("%5D", "]").Replace("%27", "'");
        }

        /// <inheritdoc/>
        public ResourceLinks GetResourceLinks(string resourceName, string id)
        {
            if (resourceName == null) throw new ArgumentNullException(nameof(resourceName));
            if (id == null) throw new ArgumentNullException(nameof(id));

            var resourceContext = _provider.GetResourceContext(resourceName);
            if (ShouldAddResourceLink(resourceContext, Links.Self))
            {
                return new ResourceLinks { Self = GetSelfResourceLink(resourceName, id) };
            }

            return null;
        }

        /// <inheritdoc/>
        public RelationshipLinks GetRelationshipLinks(RelationshipAttribute relationship, IIdentifiable parent)
        {
            if (relationship == null) throw new ArgumentNullException(nameof(relationship));
            if (parent == null) throw new ArgumentNullException(nameof(parent));

            var parentResourceContext = _provider.GetResourceContext(parent.GetType());
            var childNavigation = relationship.PublicName;
            RelationshipLinks links = null;
            if (ShouldAddRelationshipLink(parentResourceContext, relationship, Links.Related))
            {
                links = new RelationshipLinks { Related = GetRelatedRelationshipLink(parentResourceContext.ResourceName, parent.StringId, childNavigation) };
            }

            if (ShouldAddRelationshipLink(parentResourceContext, relationship, Links.Self))
            {
                links ??= new RelationshipLinks();
                links.Self = GetSelfRelationshipLink(parentResourceContext.ResourceName, parent.StringId, childNavigation);
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
        private bool ShouldAddResourceLink(ResourceContext resourceContext, Links link)
        {
            if (resourceContext.ResourceLinks != Links.NotConfigured)
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
        private bool ShouldAddRelationshipLink(ResourceContext resourceContext, RelationshipAttribute relationship, Links link)
        {
            if (relationship.RelationshipLinks != Links.NotConfigured)
            {
                return relationship.RelationshipLinks.HasFlag(link);
            }
            if (resourceContext.RelationshipLinks != Links.NotConfigured)
            {
                return resourceContext.RelationshipLinks.HasFlag(link);
            }

            return _options.RelationshipLinks.HasFlag(link);
        }
    }
}
