using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Configuration
{
    /// <summary>
    /// Responsible for populating <see cref="RelationshipAttribute.InverseNavigationProperty" />. This service is instantiated in the configure phase of the
    /// application. When using a data access layer different from EF Core, and when using ResourceHooks that depend on the inverse navigation property
    /// (BeforeImplicitUpdateRelationship), you will need to override this service, or set <see cref="RelationshipAttribute.InverseNavigationProperty" />
    /// explicitly.
    /// </summary>
    [PublicAPI]
    public interface IInverseNavigationResolver
    {
        /// <summary>
        /// This method is called upon startup by JsonApiDotNetCore. It resolves inverse navigations.
        /// </summary>
        void Resolve();
    }
}
