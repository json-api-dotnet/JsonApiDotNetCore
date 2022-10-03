using JetBrains.Annotations;

namespace JsonApiDotNetCore.Resources.Annotations;

/// <summary>
/// A simplified version, provided for convenience to multi-target against NetStandard. Does not actually work with JsonApiDotNetCore.
/// </summary>
[PublicAPI]
public abstract class ResourceFieldAttribute : Attribute
{
    /// <summary />
    public string PublicName { get; set; } = null!;
}
