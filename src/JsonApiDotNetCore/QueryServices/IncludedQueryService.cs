using System.Collections.Generic;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Services
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
