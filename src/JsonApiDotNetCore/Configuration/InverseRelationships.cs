using System;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace JsonApiDotNetCore.Configuration
{
    /// <inheritdoc />
    public class InverseRelationships : IInverseRelationships
    {
        private readonly IResourceContextProvider _provider;
        private readonly IDbContextResolver _resolver;

        public InverseRelationships(IResourceContextProvider provider, IDbContextResolver resolver = null)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _resolver = resolver;
        }

        /// <inheritdoc />
        public void Resolve()
        {
            if (IsEntityFrameworkCoreEnabled())
            {
                DbContext context = _resolver.GetContext();

                foreach (ResourceContext ce in _provider.GetResourceContexts())
                {
                    IEntityType meta = context.Model.FindEntityType(ce.ResourceType);
                    if (meta == null) continue;
                    foreach (var attr in ce.Relationships)
                    {
                        if (attr is HasManyThroughAttribute) continue;
                        INavigation inverseNavigation = meta.FindNavigation(attr.Property.Name)?.FindInverse();
                        attr.InverseNavigation = inverseNavigation?.Name;
                    }
                }
            }
        }

        // If EF Core is not being used, we're expecting the resolver to not be registered.
        private bool IsEntityFrameworkCoreEnabled() => _resolver != null;
    }
}
