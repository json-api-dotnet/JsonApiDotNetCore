using JsonApiDotNetCore.Managers.Contracts;
using JsonApiDotNetCore.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace JsonApiDotNetCore.Managers
{
    class QueryManager : IQueryManager
    {
        private IJsonApiContext _jsonApiContext;

        public QueryManager(IJsonApiContext jsonApiContext)
        {
            _jsonApiContext = jsonApiContext;
        }

        public List<string> GetFields()
        {
            return _jsonApiContext.QuerySet?.Fields;
        }

        public List<string> GetRelationships()
        {
            return _jsonApiContext.QuerySet?.IncludedRelationships;
        }
    }
}
