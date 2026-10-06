using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Configuration;

/// <summary>
/// Responsible for populating <see cref="RelationshipAttribute.InverseNavigationProperty" />. This service is instantiated at startup. When using a data
/// access layer other than Entity Framework Core, you should implement and register this service.
/// </summary>
[PublicAPI]
public interface IInverseNavigationResolver
{
    /// <summary>
    /// Resolves the opposite direction of relationships in the resource graph and populates their
    /// <see cref="RelationshipAttribute.InverseNavigationProperty" />.
    /// </summary>
    void Resolve();
}
