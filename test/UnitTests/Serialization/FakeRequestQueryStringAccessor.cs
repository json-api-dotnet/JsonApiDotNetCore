using JsonApiDotNetCore.QueryStrings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;

namespace UnitTests.Serialization
{
    internal sealed class FakeRequestQueryStringAccessor : IRequestQueryStringAccessor
    {
        public IQueryCollection Query { get; }

        public FakeRequestQueryStringAccessor()
            : this(null)
        {
        }

        public FakeRequestQueryStringAccessor(string queryString)
        {
            Query = string.IsNullOrEmpty(queryString) ? QueryCollection.Empty : new QueryCollection(QueryHelpers.ParseQuery(queryString));
        }
    }
}
