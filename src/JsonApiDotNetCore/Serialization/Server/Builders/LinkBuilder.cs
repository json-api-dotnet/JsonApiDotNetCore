using System.Text;
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
        private readonly IResourceContextProvider _provider;
        private readonly ILinksConfiguration _options;
        private readonly ICurrentRequest _currentRequest;
        private readonly IPageService _pageService;

        public LinkBuilder(ILinksConfiguration options,
                           ICurrentRequest currentRequest,
                           IPageService pageService,
                           IResourceContextProvider provider)
        {
            _options = options;
            _currentRequest = currentRequest;
            _pageService = pageService;
            _provider = provider;
        }

        /// <inheritdoc/>
        public TopLevelLinks GetTopLevelLinks()
        {
            ResourceContext resourceContext = _currentRequest.GetRequestResource();

            TopLevelLinks topLevelLinks = null;
            if (ShouldAddTopLevelLink(resourceContext, Link.Self))
            {
                topLevelLinks = new TopLevelLinks { Self = GetSelfTopLevelLink(resourceContext) };
            }

            if (ShouldAddTopLevelLink(resourceContext, Link.Paging) && _pageService.CanPaginate)
            {   
                SetPageLinks(resourceContext, topLevelLinks ??= new TopLevelLinks());
            }

            return topLevelLinks;
        }

        /// <summary>
        /// Checks if the top-level <paramref name="link"/> should be added by first checking
        /// configuration on the <see cref="ResourceContext"/>, and if not configured, by checking with the
        /// global configuration in <see cref="ILinksConfiguration"/>.
        /// </summary>
        private bool ShouldAddTopLevelLink(ResourceContext resourceContext, Link link)
        {
            if (resourceContext.TopLevelLinks != Link.NotConfigured)
            {
                return resourceContext.TopLevelLinks.HasFlag(link);
            }

            return _options.TopLevelLinks.HasFlag(link);
        }

        private void SetPageLinks(ResourceContext resourceContext, TopLevelLinks links)
        {
            if (_pageService.CurrentPage > 1)
            {
                links.Prev = GetPageLink(resourceContext, _pageService.CurrentPage - 1, _pageService.CurrentPageSize);
            }

            if (_pageService.CurrentPage < _pageService.TotalPages)
            {
                links.Next = GetPageLink(resourceContext, _pageService.CurrentPage + 1, _pageService.CurrentPageSize);
            }

            if (_pageService.TotalPages > 0)
            {
                links.Self = GetPageLink(resourceContext, _pageService.CurrentPage, _pageService.CurrentPageSize);
                links.First = GetPageLink(resourceContext, 1, _pageService.CurrentPageSize);
                links.Last = GetPageLink(resourceContext, _pageService.TotalPages, _pageService.CurrentPageSize);
            }
        }

        private string GetSelfTopLevelLink(ResourceContext resourceContext)
        {
            var builder = new StringBuilder();
            builder.Append(GetBasePath());
            builder.Append("/");
            builder.Append(resourceContext.ResourceName);

            string resourceId = _currentRequest.BaseId;
            if (resourceId != null)
            {
                builder.Append("/");
                builder.Append(resourceId);
            }

            if (_currentRequest.RequestRelationship != null)
            {
                builder.Append("/");
                builder.Append(_currentRequest.RequestRelationship.PublicRelationshipName);
            }

            return builder.ToString();
        }

        private string GetPageLink(ResourceContext resourceContext, int pageOffset, int pageSize)
        {
            if (_pageService.Backwards)
            {
                pageOffset = -pageOffset;
            }

            return $"{GetBasePath()}/{resourceContext.ResourceName}?page[size]={pageSize}&page[number]={pageOffset}";
        }


        /// <inheritdoc/>
        public ResourceLinks GetResourceLinks(string resourceName, string id)
        {
            var resourceContext = _provider.GetResourceContext(resourceName);
            if (ShouldAddResourceLink(resourceContext, Link.Self))
            {
                return new ResourceLinks { Self = GetSelfResourceLink(resourceName, id) };
            }

            return null;
        }

        /// <inheritdoc/>
        public RelationshipLinks GetRelationshipLinks(RelationshipAttribute relationship, IIdentifiable parent)
        {
            var parentResourceContext = _provider.GetResourceContext(parent.GetType());
            var childNavigation = relationship.PublicRelationshipName;
            RelationshipLinks links = null;
            if (ShouldAddRelationshipLink(parentResourceContext, relationship, Link.Related))
            {
                links = new RelationshipLinks { Related = GetRelatedRelationshipLink(parentResourceContext.ResourceName, parent.StringId, childNavigation) };
            }

            if (ShouldAddRelationshipLink(parentResourceContext, relationship, Link.Self))
            {
                links ??= new RelationshipLinks();
                links.Self = GetSelfRelationshipLink(parentResourceContext.ResourceName, parent.StringId, childNavigation);
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
        /// configuration on the <see cref="ResourceContext"/>, and if not configured, by checking with the
        /// global configuration in <see cref="ILinksConfiguration"/>.
        /// </summary>
        private bool ShouldAddResourceLink(ResourceContext resourceContext, Link link)
        {
            if (resourceContext.ResourceLinks != Link.NotConfigured)
            {
                return resourceContext.ResourceLinks.HasFlag(link);
            }
            return _options.ResourceLinks.HasFlag(link);
        }

        /// <summary>
        /// Checks if the resource object level <paramref name="link"/> should be added by first checking
        /// configuration on the <paramref name="relationship"/> attribute, if not configured by checking
        /// the <see cref="ResourceContext"/>, and if not configured by checking with the
        /// global configuration in <see cref="ILinksConfiguration"/>.
        /// </summary>
        private bool ShouldAddRelationshipLink(ResourceContext resourceContext, RelationshipAttribute relationship, Link link)
        {
            if (relationship.RelationshipLinks != Link.NotConfigured)
            {
                return relationship.RelationshipLinks.HasFlag(link);
            }
            if (resourceContext.RelationshipLinks != Link.NotConfigured)
            {
                return resourceContext.RelationshipLinks.HasFlag(link);
            }

            return _options.RelationshipLinks.HasFlag(link);
        }

        protected string GetBasePath()
        {
            if (_options.RelativeLinks)
            {
                return string.Empty;
            }

            return _currentRequest.BasePath;
        }
    }
}
