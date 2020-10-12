using System;
using System.Net;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Serialization.Objects;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Errors
{
    /// <summary>
    /// The error that is thrown when the repository layer fails to save a new state.
    /// </summary>
    public sealed class RepositorySaveException : Exception
    {
        public RepositorySaveException(Exception exception) : base(exception.Message, exception) { }
    }
}
