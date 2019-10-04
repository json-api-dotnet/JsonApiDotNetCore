using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Managers.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Links;
using JsonApiDotNetCore.Query;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.Serialization.Server.Builders
{

    public class LinkBuilder : ILinkBuilder
    {
        protected readonly ICurrentRequest _currentRequest;
        protected readonly ILinksConfiguration _options;
        protected readonly IContextEntityProvider _provider;

        public LinkBuilder(ILinksConfiguration options,
                           ICurrentRequest currentRequest,
                           IContextEntityProvider provider)
        {
            _options = options;
            _currentRequest = currentRequest;
            _provider = provider;
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

        protected string GetBasePath()
        {
            if (_options.RelativeLinks)
                return string.Empty;
            return _currentRequest.BasePath;
        }
    }

    /// <inheritdoc/>
    public class PrimaryLinkBuilder<TResource> : LinkBuilder, IPrimaryLinkBuilder<TResource> where TResource : class, IIdentifiable
    {
        private readonly ContextEntity _primaryResource;
        private readonly IPageQueryService _pageManager;

        public PrimaryLinkBuilder(ILinksConfiguration options,
                                  ICurrentRequest currentRequest,
                                  IPageQueryService pageManager,
                                  IContextEntityProvider provider)
            : base(options, currentRequest, provider)
        {
            _primaryResource = _provider.GetContextEntity<TResource>();
            _pageManager = pageManager;
        }

        /// <inheritdoc/>
        public TopLevelLinks GetTopLevelLinks()
        {
            TopLevelLinks topLevelLinks = null;
            if (ShouldAddTopLevelLink(Link.Self))
                topLevelLinks = new TopLevelLinks { Self = GetSelfTopLevelLink(_primaryResource.EntityName) };

            if (ShouldAddTopLevelLink(Link.Paging))
                SetPageLinks(ref topLevelLinks);

            return topLevelLinks;
        }

        /// <summary>
        /// Checks if the top-level <paramref name="link"/> should be added by first checking
        /// configuration on the <see cref="ContextEntity"/>, and if not configured, by checking with the
        /// global configuration in <see cref="ILinksConfiguration"/>.
        /// </summary>
        /// <param name="link"></param>
        private bool ShouldAddTopLevelLink(Link link)
        {
            if (_primaryResource.TopLevelLinks != Link.NotConfigured)
                return _primaryResource.TopLevelLinks.HasFlag(link);
            return _options.TopLevelLinks.HasFlag(link);
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


        private string GetPageLink(int pageOffset, int pageSize)
        {
            var filterQueryComposer = new QueryComposer();
            var filters = filterQueryComposer.Compose(_currentRequest);
            return $"{GetBasePath()}/{_primaryResource.EntityName}?page[size]={pageSize}&page[number]={pageOffset}{filters}";
        }
    }
}
