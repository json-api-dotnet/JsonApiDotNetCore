using JetBrains.Annotations;

namespace JsonApiDotNetCore.Resources.Annotations;

/// <summary>
/// When put on a resource class, overrides global configuration for which links to render.
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class ResourceLinksAttribute : Attribute
{
    /// <summary>
    /// Configures which links to write in the top-level links object for this resource type. Defaults to <see cref="LinkTypes.NotConfigured" />, which falls
    /// back to TopLevelLinks in global options.
    /// </summary>
    public LinkTypes TopLevelLinks { get; set; } = LinkTypes.NotConfigured;

    /// <summary>
    /// Configures which links to write in the resource-level links object for this resource type. Defaults to <see cref="LinkTypes.NotConfigured" />, which
    /// falls back to ResourceLinks in global options.
    /// </summary>
    public LinkTypes ResourceLinks { get; set; } = LinkTypes.NotConfigured;

    /// <summary>
    /// Configures which links to write in the relationship-level links object for all relationships of this resource type. Defaults to
    /// <see cref="LinkTypes.NotConfigured" />, which falls back to RelationshipLinks in global options. This can be overruled per relationship by setting
    /// <see cref="RelationshipAttribute.Links" />.
    /// </summary>
    public LinkTypes RelationshipLinks { get; set; } = LinkTypes.NotConfigured;
}
