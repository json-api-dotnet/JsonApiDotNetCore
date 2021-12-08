using JetBrains.Annotations;

// ReSharper disable CheckNamespace
#pragma warning disable AV1505 // Namespace should match with assembly name

namespace JsonApiDotNetCore.Resources;

/// <summary>
/// Represents a stripped-down copy of this type in the JsonApiDotNetCore project. It exists solely to fulfill the dependency needs for successfully
/// compiling the source-generated controllers in this project.
/// </summary>
[PublicAPI]
public abstract class Identifiable<TId> : IIdentifiable<TId>
{
    public virtual TId Id { get; set; } = default!;

    public string? StringId { get; set; }
    public string? LocalId { get; set; }
}
