using System;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.Queries.Internal.Parsing
{
    [PublicAPI]
    public sealed class QueryParseException : Exception
    {
        public QueryParseException(string message)
            : base(message)
        {
        }
    }
}
