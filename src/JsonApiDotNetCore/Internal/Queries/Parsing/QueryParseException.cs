using System;

namespace JsonApiDotNetCore.Internal.Queries.Parsing
{
    public sealed class QueryParseException : Exception
    {
        public QueryParseException(string message) : base(message)
        {
        }
    }
}
