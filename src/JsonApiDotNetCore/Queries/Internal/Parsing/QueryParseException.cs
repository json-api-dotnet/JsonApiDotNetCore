using System;

namespace JsonApiDotNetCore.Queries.Internal.Parsing
{
    public sealed class QueryParseException : Exception
    {
        public QueryParseException(string message)
            : base(message)
        {
        }
    }
}
