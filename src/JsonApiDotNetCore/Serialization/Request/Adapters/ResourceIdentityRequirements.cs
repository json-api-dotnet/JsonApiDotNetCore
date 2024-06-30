using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.Request.Adapters;

/// <summary>
/// Defines requirements to validate a <see cref="ResourceIdentity" /> instance against.
/// </summary>
[PublicAPI]
public sealed class ResourceIdentityRequirements
{
    /// <summary>
    /// When not null, indicates that the "type" element must be compatible with the specified resource type.
    /// </summary>
    public ResourceType? ResourceType { get; init; }

    /// <summary>
    /// When not null, provides a callback to indicate the presence or absence of the "id" element.
    /// </summary>
    public Func<ResourceType, JsonElementConstraint?>? EvaluateIdConstraint { get; init; }

    /// <summary>
    /// When not null, provides a callback to indicate whether the "lid" element can be used instead of the "id" element. Defaults to <c>false</c>.
    /// </summary>
    public Func<ResourceType, bool>? EvaluateAllowLid { get; init; }

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

    internal static JsonElementConstraint? DoEvaluateIdConstraint(ResourceType resourceType, WriteOperationKind? writeOperation,
        ClientIdGenerationMode globalClientIdGeneration)
    {
        ClientIdGenerationMode clientIdGeneration = resourceType.ClientIdGeneration ?? globalClientIdGeneration;

        return writeOperation == WriteOperationKind.CreateResource
            ? clientIdGeneration switch
            {
                ClientIdGenerationMode.Required => JsonElementConstraint.Required,
                ClientIdGenerationMode.Forbidden => JsonElementConstraint.Forbidden,
                _ => null
            }
            : JsonElementConstraint.Required;
    }

    internal static bool DoEvaluateAllowLid(ResourceType resourceType, WriteOperationKind? writeOperation, ClientIdGenerationMode globalClientIdGeneration)
    {
        if (writeOperation == null)
        {
            return false;
        }

        ClientIdGenerationMode clientIdGeneration = resourceType.ClientIdGeneration ?? globalClientIdGeneration;
        return !(writeOperation == WriteOperationKind.CreateResource && clientIdGeneration == ClientIdGenerationMode.Required);
    }
}
