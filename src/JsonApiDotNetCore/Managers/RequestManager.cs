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
    public class UpdatesContainer
    {
        /// <summary>
        /// The attributes that were included in a PATCH request. 
        /// Only the attributes in this dictionary should be updated.
        /// </summary>
        public Dictionary<AttrAttribute, object> Attributes { get; set; } = new Dictionary<AttrAttribute, object>();

        /// <summary>
        /// Any relationships that were included in a PATCH request. 
        /// Only the relationships in this dictionary should be updated.
        /// </summary>
        public Dictionary<RelationshipAttribute, object> Relationships { get; } = new Dictionary<RelationshipAttribute, object>();

    }

    class RequestManager : IRequestManager
    {
        private ContextEntity _contextEntity;
        private IQueryParser _queryParser;

        public string BasePath { get; set; }
        public List<string> IncludedRelationships { get; set; }
        public QuerySet QuerySet { get; set; }
        public PageManager PageManager { get; set; }
        public IQueryCollection FullQuerySet { get; set; }
        public QueryParams DisabledQueryParams { get; set; }
        public bool IsRelationshipPath { get; set; }
        public Dictionary<AttrAttribute, object> AttributesToUpdate { get; set; }
        /// <summary>
        /// Contains all the information you want about any update occuring
        /// </summary>
        private UpdatesContainer _updatesContainer { get; set; } = new UpdatesContainer();
        public Dictionary<RelationshipAttribute, object> RelationshipsToUpdate { get; set; }


        public Dictionary<AttrAttribute, object> GetUpdatedAttributes()
        {
            return _updatesContainer.Attributes;
        }
        public Dictionary<RelationshipAttribute, object> GetUpdatedRelationships()
        {
            return _updatesContainer.Relationships;
        }
        public List<string> GetFields()
        {
            return QuerySet?.Fields;
        }

        public List<string> GetRelationships()
        {
            return QuerySet?.IncludedRelationships;
        }
        public ContextEntity GetContextEntity()
        {
            return _contextEntity;
        }

        public void SetContextEntity(ContextEntity contextEntityCurrent)
        {
            _contextEntity = contextEntityCurrent;
        }
    }
}
