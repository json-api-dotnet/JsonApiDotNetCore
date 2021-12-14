using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Configuration;

/// <summary>
/// Responsible for populating <see cref="RelationshipAttribute.InverseNavigationProperty" />. This service is instantiated in the configure phase of the
/// application. When using a data access layer different from Entity Framework Core, you will need to implement and register this service, or set
/// <see cref="RelationshipAttribute.InverseNavigationProperty" /> explicitly.
/// </summary>
[PublicAPI]
public interface IInverseNavigationResolver
{
    /// <summary>
    /// This method is called upon startup by JsonApiDotNetCore. It resolves inverse navigations.
    /// </summary>
    void Resolve();
}
