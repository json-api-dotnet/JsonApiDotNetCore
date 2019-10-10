using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Query;
using JsonApiDotNetCore.Managers.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Query;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace JsonApiDotNetCore.Managers
{

    class CurrentRequest : ICurrentRequest
    {
        private ContextEntity _contextEntity;
        public string BasePath { get; set; }
        public List<string> IncludedRelationships { get; set; }
        public QuerySet QuerySet { get; set; }
        public PageService PageManager { get; set; }
        public IQueryCollection FullQuerySet { get; set; }
        public QueryParams DisabledQueryParams { get; set; }
        public bool IsRelationshipPath { get; set; }
        public Dictionary<AttrAttribute, object> AttributesToUpdate { get; set; }

        public Dictionary<RelationshipAttribute, object> RelationshipsToUpdate { get; set; }

        public RelationshipAttribute RequestRelationship { get; set; }

        public List<string> GetFields()
        {
            return QuerySet?.Fields;
        }

        public List<string> GetRelationships()
        {
            return QuerySet?.IncludedRelationships;
        }

        /// <summary>
        /// The main resource of the request.
        /// </summary>
        /// <returns></returns>
        public ContextEntity GetRequestResource()
        {
            return _contextEntity;
        }

        public void SetRequestResource(ContextEntity primaryResource)
        {
            _contextEntity = primaryResource;
        }
    }
}
