using JetBrains.Annotations;

namespace JsonApiDotNetCore.Resources.Annotations;

/// <summary>
/// Indicates what can be performed on a <see cref="HasOneAttribute" />.
/// </summary>
[PublicAPI]
[Flags]
public enum HasOneCapabilities
{
    None = 0,

    /// <summary>
    /// Whether or not the relationship can be returned in responses. Attempts to explicitly request it via the <c>fields</c> query string parameter when
    /// disabled will return an HTTP 400 response. Otherwise, the relationship (and its related resources, when included) are silently omitted.
    /// </summary>
    /// <remarks>
    /// Note this setting does not affect retrieving the related resources directly.
    /// </remarks>
    AllowView = 1,

    /// <summary>
    /// Whether or not the relationship can be included. Attempts to use it in the <c>include</c> query string parameter when disabled will return an HTTP
    /// 400 response.
    /// </summary>
    AllowInclude = 1 << 1,

    /// <summary>
    /// Whether or not POST and PATCH requests can replace the relationship. Attempts to replace when disabled will return an HTTP 422 response.
    /// </summary>
    AllowSet = 1 << 2,

    All = AllowView | AllowInclude | AllowSet
}
