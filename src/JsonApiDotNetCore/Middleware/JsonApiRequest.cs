using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Middleware;

/// <inheritdoc />
[PublicAPI]
public sealed class JsonApiRequest : IJsonApiRequest
{
    /// <inheritdoc />
    public EndpointKind Kind { get; set; }

    /// <inheritdoc />
    public string? PrimaryId { get; set; }

    /// <inheritdoc />
    public ResourceType? PrimaryResourceType { get; set; }

    /// <inheritdoc />
    public ResourceType? SecondaryResourceType { get; set; }

    /// <inheritdoc />
    public RelationshipAttribute? Relationship { get; set; }

    /// <inheritdoc />
    public bool IsCollection { get; set; }

    /// <inheritdoc />
    public bool IsReadOnly { get; set; }

    /// <inheritdoc />
    public WriteOperationKind? WriteOperation { get; set; }

    /// <inheritdoc />
    public string? TransactionId { get; set; }

    /// <inheritdoc />
    public void CopyFrom(IJsonApiRequest other)
    {
        ArgumentGuard.NotNull(other, nameof(other));

        Kind = other.Kind;
        PrimaryId = other.PrimaryId;
        PrimaryResourceType = other.PrimaryResourceType;
        SecondaryResourceType = other.SecondaryResourceType;
        Relationship = other.Relationship;
        IsCollection = other.IsCollection;
        IsReadOnly = other.IsReadOnly;
        WriteOperation = other.WriteOperation;
        TransactionId = other.TransactionId;
    }
}
