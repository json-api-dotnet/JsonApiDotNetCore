using System.Collections.Generic;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.QueryServices.Contracts;

namespace JsonApiDotNetCore.QueryServices
{

    public class IncludedQueryService : IIncludedQueryService, IInternalIncludedQueryService
    {
        private readonly List<List<RelationshipAttribute>> _includedChains;

        public IncludedQueryService()
        {
            _includedChains = new List<List<RelationshipAttribute>>();
        }

        public List<List<RelationshipAttribute>> Get()
        {
            return _includedChains;
        }

        public void Register(List<RelationshipAttribute> chain)
        {
            _includedChains.Add(chain);
        }
    }
}
