namespace JsonApiDotNetCore.Serialization.Response;

/// <summary>
/// Provides no value for the "describedby" link in https://jsonapi.org/format/#document-top-level.
/// </summary>
public sealed class NoDocumentDescriptionLinkProvider : IDocumentDescriptionLinkProvider
{
    /// <summary>
    /// Always returns <c>null</c>.
    /// </summary>
    public string? GetUrl()
    {
        return null;
    }
}
