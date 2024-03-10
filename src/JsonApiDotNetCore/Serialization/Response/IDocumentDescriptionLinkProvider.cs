using JsonApiDotNetCore.Configuration;

namespace JsonApiDotNetCore.Serialization.Response;

/// <summary>
/// Provides the value for the "describedby" link in https://jsonapi.org/format/#document-top-level.
/// </summary>
public interface IDocumentDescriptionLinkProvider
{
    /// <summary>
    /// Gets the URL for the "describedby" link, or <c>null</c> when unavailable.
    /// </summary>
    /// <remarks>
    /// The returned URL can be absolute or relative. If possible, it gets converted based on <see cref="IJsonApiOptions.UseRelativeLinks" />.
    /// </remarks>
    string? GetUrl();
}
