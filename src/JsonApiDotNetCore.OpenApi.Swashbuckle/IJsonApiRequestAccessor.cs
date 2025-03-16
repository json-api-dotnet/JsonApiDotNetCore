using JsonApiDotNetCore.Middleware;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle;

/// <summary>
/// Provides access to the current <see cref="IJsonApiRequest" />, if one is available.
/// </summary>
internal interface IJsonApiRequestAccessor
{
    /// <summary>
    /// Gets the current <see cref="IJsonApiRequest" />. Returns <c>null</c> if there is no active request.
    /// </summary>
    IJsonApiRequest? Current { get; }
}
