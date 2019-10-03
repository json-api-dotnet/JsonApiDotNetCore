using System;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Managers.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Links;
using JsonApiDotNetCore.QueryServices.Contracts;
using JsonApiDotNetCore.Serialization.Serializer.Contracts;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.Serialization.Serializer
{
    /// <inheritdoc/>
    public class LinkBuilder : ILinkBuilder
    {
        private readonly ICurrentRequest _currentRequest;
        private readonly ILinksConfiguration _options;
        private readonly IPageQueryService _pageManager;
        private readonly ContextEntity _requestResourceContext;
        private readonly IContextEntityProvider _provider;

        public LinkBuilder(ILinksConfiguration options,
                           ICurrentRequest currentRequest,
                           IPageQueryService pageManager,
                           IContextEntityProvider provider)
        {
            _options = options;
            _currentRequest = currentRequest;
            _pageManager = pageManager;
            _provider = provider;
            _requestResourceContext = _currentRequest.GetRequestResource();
        }

        /// <inheritdoc/>
        public TopLevelLinks GetTopLevelLinks()
        {
            TopLevelLinks topLevelLinks = null;
            if (ShouldAddTopLevelLink(Link.Self))
                topLevelLinks = new TopLevelLinks { Self = GetSelfTopLevelLink(_requestResourceContext.EntityName) };

            if (ShouldAddTopLevelLink(Link.Paging))
                SetPageLinks(ref topLevelLinks);

            return topLevelLinks;
        }

        /// <inheritdoc/>
        public ResourceLinks GetResourceLinks(string resourceName, string id)
        {
            var resourceContext = _provider.GetContextEntity(resourceName);
            if (ShouldAddResourceLink(resourceContext, Link.Self))
                return new ResourceLinks { Self = GetSelfResourceLink(resourceName, id) };

            return null;
        }

        /// <inheritdoc/>
        public RelationshipLinks GetRelationshipLinks(RelationshipAttribute relationship, IIdentifiable parent)
        {
            var parentResourceContext = _provider.GetContextEntity(parent.GetType());
            var childNavigation = relationship.PublicRelationshipName;
            RelationshipLinks links = null;
            if (ShouldAddRelationshipLink(parentResourceContext, relationship, Link.Related))
                links = new RelationshipLinks { Related = GetRelatedRelationshipLink(parentResourceContext.EntityName, parent.StringId, childNavigation) };

            if (ShouldAddRelationshipLink(parentResourceContext, relationship, Link.Self))
            {
                links = links ?? new RelationshipLinks();
                links.Self = GetSelfRelationshipLink(parentResourceContext.EntityName, parent.StringId, childNavigation);
            }

            return links;
        }

        private void SetPageLinks(ref TopLevelLinks links)
        {
            if (!_pageManager.ShouldPaginate())
                return;

            links = links ?? new TopLevelLinks();

            if (_pageManager.CurrentPage > 1)
            {
                links.First = GetPageLink(1, _pageManager.PageSize);
                links.Prev = GetPageLink(_pageManager.CurrentPage - 1, _pageManager.PageSize);
            }


            if (_pageManager.CurrentPage < _pageManager.TotalPages)
                links.Next = GetPageLink(_pageManager.CurrentPage + 1, _pageManager.PageSize);


            if (_pageManager.TotalPages > 0)
                links.Last = GetPageLink(_pageManager.TotalPages, _pageManager.PageSize);
        }

        private string GetSelfTopLevelLink(string resourceName)
        {
            return $"{GetBasePath()}/{resourceName}";
        }

        private string GetSelfRelationshipLink(string parent, string parentId, string navigation)
        {
            return $"{GetBasePath()}/{parent}/{parentId}/relationships/{navigation}";
        }

        private string GetSelfResourceLink(string resource, string resourceId)
        {
            return $"{GetBasePath()}/{resource}/{resourceId}";
        }

        private string GetRelatedRelationshipLink(string parent, string parentId, string navigation)
        {
            return $"{GetBasePath()}/{parent}/{parentId}/{navigation}";
        }

        private string GetPageLink(int pageOffset, int pageSize)
        {
            var filterQueryComposer = new QueryComposer();
            var filters = filterQueryComposer.Compose(_currentRequest);
            return $"{GetBasePath()}/{_requestResourceContext.EntityName}?page[size]={pageSize}&page[number]={pageOffset}{filters}";
        }

        /// <summary>
        /// Checks if the top-level <paramref name="link"/> should be added by first checking
        /// configuration on the <see cref="ContextEntity"/>, and if not configured, by checking with the
        /// global configuration in <see cref="ILinksConfiguration"/>.
        /// </summary>
        /// <param name="link"></param>
        private bool ShouldAddTopLevelLink(Link link)
        {
            if (_requestResourceContext.TopLevelLinks != Link.NotConfigured)
                return _requestResourceContext.TopLevelLinks.HasFlag(link);
            return _options.TopLevelLinks.HasFlag(link);
        }

        /// <summary>
        /// Checks if the resource object level <paramref name="link"/> should be added by first checking
        /// configuration on the <see cref="ContextEntity"/>, and if not configured, by checking with the
        /// global configuration in <see cref="ILinksConfiguration"/>.
        /// </summary>
        /// <param name="link"></param>
        private bool ShouldAddResourceLink(ContextEntity resourceContext, Link link)
        {
            if (resourceContext.ResourceLinks != Link.NotConfigured)
                return resourceContext.ResourceLinks.HasFlag(link);
            return _options.ResourceLinks.HasFlag(link);
        }

        /// <summary>
        /// Checks if the resource object level <paramref name="link"/> should be added by first checking
        /// configuration on the <paramref name="relationship"/> attribute, if not configured by checking
        /// the <see cref="ContextEntity"/>, and if not configured by checking with the
        /// global configuration in <see cref="ILinksConfiguration"/>.
        /// </summary>
        /// <param name="link"></param>
        private bool ShouldAddRelationshipLink(ContextEntity resourceContext, RelationshipAttribute relationship, Link link)
        {
            if (relationship.RelationshipLinks != Link.NotConfigured)
                return relationship.RelationshipLinks.HasFlag(link);
            if (resourceContext.RelationshipLinks != Link.NotConfigured)
                return resourceContext.RelationshipLinks.HasFlag(link);
            return _options.RelationshipLinks.HasFlag(link);
        }

        private string GetBasePath()
        {
            if (_options.RelativeLinks)
                return string.Empty;
            return _currentRequest.BasePath;
        }
    }
}
