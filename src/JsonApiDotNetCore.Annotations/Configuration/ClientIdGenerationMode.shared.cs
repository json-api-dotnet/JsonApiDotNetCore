using JetBrains.Annotations;

namespace JsonApiDotNetCore.Configuration;

/// <summary>
/// Indicates how to handle IDs sent by JSON:API clients when creating resources.
/// </summary>
[PublicAPI]
public enum ClientIdGenerationMode
{
    /// <summary>
    /// Returns an HTTP 403 (Forbidden) response if a client attempts to create a resource with a client-supplied ID.
    /// </summary>
    Forbidden,

    /// <summary>
    /// Allows a client to create a resource with a client-supplied ID, but does not require it.
    /// </summary>
    Allowed,

    /// <summary>
    /// Returns an HTTP 422 (Unprocessable Content) response if a client attempts to create a resource without a client-supplied ID.
    /// </summary>
    Required
}
