using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Query;
using JsonApiDotNetCore.Managers.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace JsonApiDotNetCore.Managers
{
    class RequestManager : IRequestManager
    {
        private ContextEntity _contextEntity;
        private IQueryParser _queryParser;

        public string BasePath { get; set; }
        public List<string> IncludedRelationships { get; set; }
        public QuerySet QuerySet { get; set; }
        public PageQueryService PageManager { get; set; }
        public IQueryCollection FullQuerySet { get; set; }
        public QueryParams DisabledQueryParams { get; set; }
        public bool IsRelationshipPath { get; set; }
        public Dictionary<AttrAttribute, object> AttributesToUpdate { get; set; }

        public Dictionary<RelationshipAttribute, object> RelationshipsToUpdate { get; set; }

        public bool IsBulkRequest { get; set; } = false;

        public List<string> GetFields()
        {
            return QuerySet?.Fields;
        }

        public List<string> GetRelationships()
        {
            return QuerySet?.IncludedRelationships;
        }

        /// <summary>s
        /// The main resource of the request.
        /// </summary>
        /// <returns></returns>
        public ContextEntity GetRequestResource()
        {
            return _contextEntity;
        }

        public void SetRequestResource(ContextEntity requestResource)
        {
            _contextEntity = requestResource;
        }
    }
}
