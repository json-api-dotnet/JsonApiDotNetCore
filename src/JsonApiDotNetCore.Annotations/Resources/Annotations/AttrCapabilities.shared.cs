using JetBrains.Annotations;

namespace JsonApiDotNetCore.Resources.Annotations;

/// <summary>
/// Indicates what can be performed on an <see cref="AttrAttribute" />.
/// </summary>
[PublicAPI]
[Flags]
public enum AttrCapabilities
{
    None = 0,

    /// <summary>
    /// Whether or not the attribute value can be returned in responses. Attempts to explicitly request it via the <c>fields</c> query string parameter when
    /// disabled will return an HTTP 400 response. Otherwise, the attribute is silently omitted.
    /// </summary>
    AllowView = 1,

    /// <summary>
    /// Whether or not POST requests can assign the attribute value. Attempts to assign when disabled will return an HTTP 422 response.
    /// </summary>
    AllowCreate = 1 << 1,

    /// <summary>
    /// Whether or not PATCH requests can update the attribute value. Attempts to update when disabled will return an HTTP 422 response.
    /// </summary>
    AllowChange = 1 << 2,

    /// <summary>
    /// Whether or not the attribute can be filtered on. Attempts to use it in the <c>filter</c> query string parameter when disabled will return an HTTP 400
    /// response.
    /// </summary>
    AllowFilter = 1 << 3,

    /// <summary>
    /// Whether or not the attribute can be sorted on. Attempts to use it in the <c>sort</c> query string parameter when disabled will return an HTTP 400
    /// response.
    /// </summary>
    AllowSort = 1 << 4,

    All = AllowView | AllowCreate | AllowChange | AllowFilter | AllowSort
}
