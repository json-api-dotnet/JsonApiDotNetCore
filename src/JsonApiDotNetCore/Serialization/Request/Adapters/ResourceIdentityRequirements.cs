using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.Request.Adapters;

/// <summary>
/// Defines requirements to validate an <see cref="IResourceIdentity" /> instance against.
/// </summary>
[PublicAPI]
public sealed class ResourceIdentityRequirements
{
    /// <summary>
    /// When not null, indicates that the "type" element must be compatible with the specified resource type.
    /// </summary>
    public ResourceType? ResourceType { get; init; }

    /// <summary>
    /// When not null, indicates the presence or absence of the "id" element.
    /// </summary>
    public JsonElementConstraint? IdConstraint { get; init; }

    /// <summary>
    /// When not null, indicates what the value of the "id" element must be.
    /// </summary>
    public string? IdValue { get; init; }

    /// <summary>
    /// When not null, indicates what the value of the "lid" element must be.
    /// </summary>
    public string? LidValue { get; init; }

    /// <summary>
    /// When not null, indicates the name of the relationship to use in error messages.
    /// </summary>
    public string? RelationshipName { get; init; }
}
