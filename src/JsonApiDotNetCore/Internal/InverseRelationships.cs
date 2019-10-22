using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Internal.Contracts;
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
        private readonly IResourceContextProvider _provider;
        private readonly IDbContextResolver _resolver;

        public InverseRelationships(IResourceContextProvider provider, IDbContextResolver resolver = null)
        {
            _provider = provider;
            _resolver = resolver;
        }

        /// <inheritdoc />
        public void Resolve()
        {
            if (EntityFrameworkCoreIsEnabled())
            {
                DbContext context = _resolver.GetContext();

                foreach (ResourceContext ce in _provider.GetContextEntities())
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
        private bool EntityFrameworkCoreIsEnabled() => _resolver != null;
    }
}
