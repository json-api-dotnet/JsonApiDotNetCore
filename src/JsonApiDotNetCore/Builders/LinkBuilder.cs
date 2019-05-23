using System;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Managers.Contracts;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCore.Builders
{
    public class LinkBuilder : ILinkBuilder
    {
        private IRequestManager _requestManager;
        private IJsonApiOptions _options;

        public LinkBuilder(IJsonApiOptions options, IRequestManager requestManager)
        {
            _requestManager = requestManager;
            _options = options;
        }


        public string GetSelfRelationLink(string parent, string parentId, string child)
        {
            return $"{_requestManager.BasePath}/{parent}/{parentId}/relationships/{child}";
        }

        public string GetRelatedRelationLink(string parent, string parentId, string child)
        {
            return $"{_requestManager.BasePath}/{parent}/{parentId}/{child}";
        }

        public string GetPageLink(int pageOffset, int pageSize)
        {
            var filterQueryComposer = new QueryComposer();
            var filters = filterQueryComposer.Compose(_requestManager);
            return $"{_requestManager.BasePath}/{_requestManager.GetContextEntity().EntityName}?page[size]={pageSize}&page[number]={pageOffset}{filters}";
        }
    }
}
