using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Managers.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Links;
using JsonApiDotNetCore.Query;

namespace JsonApiDotNetCore.Serialization.Server.Builders
{
    public class LinkBuilder : ILinkBuilder
    {
        private readonly ICurrentRequest _currentRequest;
        private readonly ILinksConfiguration _options;
        private readonly IContextEntityProvider _provider;
        private readonly IPageService _pageManager;

        public LinkBuilder(ILinksConfiguration options,
                           ICurrentRequest currentRequest,
                           IPageService pageManager,
                           IContextEntityProvider provider)
        {
            _options = options;
            _currentRequest = currentRequest;
            _pageManager = pageManager;
            _provider = provider;
        }

        /// <inheritdoc/>
        public TopLevelLinks GetTopLevelLinks(ContextEntity primaryResource)
        {
            TopLevelLinks topLevelLinks = null;
            if (ShouldAddTopLevelLink(primaryResource, Link.Self))
                topLevelLinks = new TopLevelLinks { Self = GetSelfTopLevelLink(primaryResource.EntityName) };

            if (ShouldAddTopLevelLink(primaryResource, Link.Paging))
                SetPageLinks(primaryResource, ref topLevelLinks);

            return topLevelLinks;
        }

        /// <summary>
        /// Checks if the top-level <paramref name="link"/> should be added by first checking
        /// configuration on the <see cref="ContextEntity"/>, and if not configured, by checking with the
        /// global configuration in <see cref="ILinksConfiguration"/>.
        /// </summary>
        /// <param name="link"></param>
        private bool ShouldAddTopLevelLink(ContextEntity primaryResource, Link link)
        {
            if (primaryResource.TopLevelLinks != Link.NotConfigured)
                return primaryResource.TopLevelLinks.HasFlag(link);
            return _options.TopLevelLinks.HasFlag(link);
        }

        private void SetPageLinks(ContextEntity primaryResource, ref TopLevelLinks links)
        {
            if (!_pageManager.ShouldPaginate())
                return;

            links = links ?? new TopLevelLinks();

            if (_pageManager.CurrentPage > 1)
            {
                links.First = GetPageLink(primaryResource, 1, _pageManager.PageSize);
                links.Prev = GetPageLink(primaryResource, _pageManager.CurrentPage - 1, _pageManager.PageSize);
            }

            if (_pageManager.CurrentPage < _pageManager.TotalPages)
                links.Next = GetPageLink(primaryResource, _pageManager.CurrentPage + 1, _pageManager.PageSize);


            if (_pageManager.TotalPages > 0)
                links.Last = GetPageLink(primaryResource, _pageManager.TotalPages, _pageManager.PageSize);
        }

        private string GetSelfTopLevelLink(string resourceName)
        {
            return $"{GetBasePath()}/{resourceName}";
        }

        private string GetPageLink(ContextEntity primaryResource, int pageOffset, int pageSize)
        {
            return $"{GetBasePath()}/{primaryResource.EntityName}?page[size]={pageSize}&page[number]={pageOffset}";
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
}
