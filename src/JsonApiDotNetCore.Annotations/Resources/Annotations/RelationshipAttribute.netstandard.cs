using JetBrains.Annotations;

namespace JsonApiDotNetCore.Resources.Annotations;

/// <summary>
/// A simplified version, provided for convenience to multi-target against NetStandard. Does not actually work with JsonApiDotNetCore.
/// </summary>
[PublicAPI]
public abstract class RelationshipAttribute : ResourceFieldAttribute
{
    /// <summary />
    public LinkTypes Links { get; set; } = LinkTypes.NotConfigured;

    /// <summary />
    [Obsolete("Use AllowInclude in Capabilities instead.")]
    public bool CanInclude { get; set; } = true;
}
