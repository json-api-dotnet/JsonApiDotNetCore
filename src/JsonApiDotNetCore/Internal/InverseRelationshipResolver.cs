using System;
using JsonApiDotNetCore.Data;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCore.Internal
{

    public class InverseRelationshipResolver
    {
        private readonly ResourceGraph _graph;
        private readonly IDbContextResolver _resolver;

        public InverseRelationshipResolver(ResourceGraph graph, IDbContextResolver resolver = null)
        {
            _graph = graph;
            _resolver = resolver;
        }

        internal void ResolveInverseRelationships()
        {
            if (EntityFrameworkCoreIsEnabled())
            {
                DbContext context = _resolver.GetContext();
                foreach (ContextEntity ce in _graph.Entities)
                {
                    var type = ce.EntityType;
                    foreach (var attr in ce.Relationships)
                    {

                    }
                }
            }
        }

        /// <summary>
        /// If EF Core is not being used, we're expecting the resolver to not be registered.
        /// </summary>
        /// <returns><c>true</c>, if entity framework core was enabled, <c>false</c> otherwise.</returns>
        /// <param name="resolver">Resolver.</param>
        internal bool EntityFrameworkCoreIsEnabled()
        {
            return _resolver != null;
        }
    }
}
