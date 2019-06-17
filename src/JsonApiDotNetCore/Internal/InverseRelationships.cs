using System;
using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace JsonApiDotNetCore.Internal
{
    /// <summary>
    /// Responsible for populating the RelationshipAttribute InverseNavigation property.
    /// 
    /// This service is instantiated in the configure phase of the application.
    /// 
    /// When using a data access layer different from EF Core, and when using ResourceHooks
    /// that depend on the inverse navigation property (BeforeImplicitUpdateRelationship),
    /// you will need to override this service, or pass along the inverseNavigationProperty in
    /// the RelationshipAttribute.
    /// </summary>
    public interface IInverseRelationships
    {
        /// <summary>
        /// This method is called upon startup by JsonApiDotNetCore. It should 
        /// deal with resolving the inverse relationships. 
        /// </summary>
        void Resolve();
    }

    /// <inheritdoc />
    public class InverseRelationships : IInverseRelationships
    {
        private readonly ResourceGraph _graph;
        private readonly IDbContextResolver _resolver;

        public InverseRelationships(IResourceGraph graph, IDbContextResolver resolver = null)
        {
            _graph = (ResourceGraph)graph;
            _resolver = resolver;
        }

        /// <inheritdoc />
        public void Resolve()
        {
            if (EntityFrameworkCoreIsEnabled())
            {
                DbContext context = _resolver.GetContext();

                foreach (ContextEntity ce in _graph.Entities)
                {
                    IEntityType meta = context.Model.FindEntityType(ce.EntityType);
                    if (meta == null) continue;
                    foreach (var attr in ce.Relationships)
                    {
                        if (attr is HasManyThroughAttribute) continue;
                        INavigation inverseNavigation = meta.FindNavigation(attr.InternalRelationshipName)?.FindInverse();
                        attr.InverseNavigation = inverseNavigation?.Name;
                    }
                }
            }
        }

        /// <summary>
        /// If EF Core is not being used, we're expecting the resolver to not be registered.
        /// </summary>
        /// <returns><c>true</c>, if entity framework core was enabled, <c>false</c> otherwise.</returns>
        /// <param name="resolver">Resolver.</param>
        private bool EntityFrameworkCoreIsEnabled()
        {
            return _resolver != null;
        }
    }
}
