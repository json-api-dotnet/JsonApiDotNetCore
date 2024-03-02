using JetBrains.Annotations;
using Microsoft.AspNetCore.WebUtilities;

namespace JsonApiDotNetCore.OpenApi.Client.Kiota;

/// <summary>
/// Enables setting the HTTP query string. Workaround for https://github.com/microsoft/kiota/issues/3800.
/// </summary>
[PublicAPI]
public sealed class SetQueryStringHttpMessageHandler : DelegatingHandler
{
    private IDictionary<string, string?>? _queryString;

    public IDisposable CreateScope(IDictionary<string, string?> queryString)
    {
        return new QueryStringScope(this, queryString);
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (_queryString is { Count: > 0 } && request.RequestUri != null)
        {
            request.RequestUri = new Uri(QueryHelpers.AddQueryString(request.RequestUri.ToString(), _queryString));
        }

        return base.SendAsync(request, cancellationToken);
    }

    private sealed class QueryStringScope : IDisposable
    {
        private readonly SetQueryStringHttpMessageHandler _owner;
        private readonly IDictionary<string, string?>? _backupQueryString;

        public QueryStringScope(SetQueryStringHttpMessageHandler owner, IDictionary<string, string?> queryString)
        {
            _owner = owner;
            _backupQueryString = owner._queryString;

            owner._queryString = SetEmptyStringForNullValues(queryString);
        }

        private static Dictionary<string, string?> SetEmptyStringForNullValues(IDictionary<string, string?> queryString)
        {
            // QueryHelpers.AddQueryString ignores null values, so replace them with empty strings to get them sent.
            return queryString.ToDictionary(pair => pair.Key, pair => pair.Value ?? (string?)string.Empty);
        }

        public void Dispose()
        {
            _owner._queryString = _backupQueryString;
        }
    }
}
