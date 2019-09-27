using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Managers.Contracts;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.Builders
{
    public class LinkBuilder : ILinkBuilder
    {
        private readonly IRequestManager _requestManager;
        private readonly IJsonApiOptions _options;

        public LinkBuilder(IJsonApiOptions options, IRequestManager requestManager)
        {
            _options = options;
            _requestManager = requestManager;
        }

        /// <inheritdoc/>
        public string GetSelfRelationLink(string parent, string parentId, string child)
        {
            return $"{GetBasePath()}/{parent}/{parentId}/relationships/{child}";
        }

        /// <inheritdoc/>
        public string GetRelatedRelationLink(string parent, string parentId, string child)
        {
            return $"{GetBasePath()}/{parent}/{parentId}/{child}";
        }

        /// <inheritdoc/>
        public string GetPageLink(int pageOffset, int pageSize)
        {
            var filterQueryComposer = new QueryComposer();
            var filters = filterQueryComposer.Compose(_requestManager);
            return $"{GetBasePath()}/{_requestManager.GetContextEntity().EntityName}?page[size]={pageSize}&page[number]={pageOffset}{filters}";
        }

        private string GetBasePath()
        {
            if (_options.RelativeLinks) return string.Empty;
            return _requestManager.BasePath;
        }
    }
}
