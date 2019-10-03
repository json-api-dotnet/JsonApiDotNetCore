using System.Collections.Generic;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.QueryServices.Contracts;

namespace JsonApiDotNetCore.QueryServices
{
    public class IncludeQueryService : IIncludeQueryService, IInternalIncludeQueryService
    {
        private readonly List<List<RelationshipAttribute>> _includedChains;

        public IncludeQueryService()
        {
            _includedChains = new List<List<RelationshipAttribute>>();
        }

        /// <inheritdoc/>
        public List<List<RelationshipAttribute>> Get()
        {
            return _includedChains;
        }

        /// <inheritdoc/>
        public void Register(List<RelationshipAttribute> chain)
        {
            _includedChains.Add(chain);
        }
    }
}
