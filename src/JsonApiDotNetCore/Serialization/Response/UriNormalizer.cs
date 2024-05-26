namespace JsonApiDotNetCore.Serialization.Response;

internal sealed class UriNormalizer
{
    /// <summary>
    /// Converts a URL to absolute or relative format, if possible.
    /// </summary>
    /// <param name="sourceUrl">
    /// The absolute or relative URL to normalize.
    /// </param>
    /// <param name="preferRelative">
    /// Whether to convert <paramref name="sourceUrl" /> to absolute or relative format.
    /// </param>
    /// <param name="requestUri">
    /// The URL of the current HTTP request, whose path and query string are discarded.
    /// </param>
    public string Normalize(string sourceUrl, bool preferRelative, Uri requestUri)
    {
        var sourceUri = new Uri(sourceUrl, UriKind.RelativeOrAbsolute);
        Uri baseUri = RemovePathFromAbsoluteUri(requestUri);

        if (!sourceUri.IsAbsoluteUri && !preferRelative)
        {
            var absoluteUri = new Uri(baseUri, sourceUrl);
            return absoluteUri.AbsoluteUri;
        }

        if (sourceUri.IsAbsoluteUri && preferRelative)
        {
            if (AreSameServer(baseUri, sourceUri))
            {
                Uri relativeUri = baseUri.MakeRelativeUri(sourceUri);
                return relativeUri.ToString();
            }
        }

        return sourceUrl;
    }

    private static Uri RemovePathFromAbsoluteUri(Uri uri)
    {
        var requestUriBuilder = new UriBuilder(uri)
        {
            Path = null
        };

        return requestUriBuilder.Uri;
    }

    private static bool AreSameServer(Uri left, Uri right)
    {
        // Custom implementation because Uri.Equals() ignores the casing of username/password.

        string leftScheme = left.GetComponents(UriComponents.Scheme, UriFormat.UriEscaped);
        string rightScheme = right.GetComponents(UriComponents.Scheme, UriFormat.UriEscaped);

        if (!string.Equals(leftScheme, rightScheme, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        string leftServer = left.GetComponents(UriComponents.HostAndPort, UriFormat.UriEscaped);
        string rightServer = right.GetComponents(UriComponents.HostAndPort, UriFormat.UriEscaped);

        if (!string.Equals(leftServer, rightServer, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        string leftUserInfo = left.GetComponents(UriComponents.UserInfo, UriFormat.UriEscaped);
        string rightUserInfo = right.GetComponents(UriComponents.UserInfo, UriFormat.UriEscaped);

        return leftUserInfo == rightUserInfo;
    }
}
