namespace JsonApiDotNetCore.Resources;

/// <summary>
/// A simplified version, provided for convenience to multi-target against NetStandard. Does not actually work with JsonApiDotNetCore.
/// </summary>
public abstract class Identifiable<TId> : IIdentifiable<TId>
{
    /// <summary />
    public virtual TId Id { get; set; } = default!;

    /// <summary />
    public string? StringId { get; set; }

    /// <summary />
    public string? LocalId { get; set; }
}
