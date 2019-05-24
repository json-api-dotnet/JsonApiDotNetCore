using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Query;
using JsonApiDotNetCore.Managers.Contracts;
using JsonApiDotNetCore.Services;
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
        public PageManager PageManager { get; set; }


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
