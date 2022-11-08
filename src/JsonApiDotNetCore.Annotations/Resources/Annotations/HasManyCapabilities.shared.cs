using JetBrains.Annotations;

namespace JsonApiDotNetCore.Resources.Annotations;

/// <summary>
/// Indicates what can be performed on a <see cref="HasManyAttribute" />.
/// </summary>
[PublicAPI]
[Flags]
public enum HasManyCapabilities
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
    /// Whether or not the to-many relationship can be used in the <c>count()</c> and <c>has()</c> functions as part of the <c>filter</c> query string
    /// parameter. Attempts to use it when disabled will return an HTTP 400 response.
    /// </summary>
    AllowFilter = 1 << 2,

    /// <summary>
    /// Whether or not POST and PATCH requests can replace the relationship. Attempts to replace when disabled will return an HTTP 422 response.
    /// </summary>
    AllowSet = 1 << 3,

    /// <summary>
    /// Whether or not POST requests can add to the to-many relationship. Attempts to add when disabled will return an HTTP 422 response.
    /// </summary>
    AllowAdd = 1 << 4,

    /// <summary>
    /// Whether or not DELETE requests can remove from the to-many relationship. Attempts to remove when disabled will return an HTTP 422 response.
    /// </summary>
    AllowRemove = 1 << 5,

    All = AllowView | AllowInclude | AllowFilter | AllowSet | AllowAdd | AllowRemove
}
