using JetBrains.Annotations;

namespace JsonApiDotNetCore.Resources.Annotations;

/// <summary>
/// A simplified version, provided for convenience to multi-target against NetStandard. Does not actually work with JsonApiDotNetCore.
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Property)]
public sealed class HasManyAttribute : RelationshipAttribute
{
    /// <summary />
    public HasManyCapabilities Capabilities { get; set; }

    /// <summary />
    public bool DisablePagination { get; set; }
}
