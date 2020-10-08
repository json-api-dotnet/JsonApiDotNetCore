using System;
using System.Net;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Serialization.Objects;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Errors
{
    /// <summary>
    /// The error that is thrown when Entity Framework Core fails executing a query.
    /// </summary>
    public sealed class QueryExecutionException : Exception
    {
        public QueryExecutionException(Exception exception) : base(exception.Message, exception) { }
    }
}
