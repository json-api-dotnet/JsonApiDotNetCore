#nullable disable

using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Middleware
{
    /// <summary>
    /// Central place to handle all exceptions, such as log them and translate into error response.
    /// </summary>
    public interface IExceptionHandler
    {
        IReadOnlyList<ErrorObject> HandleException(Exception exception);
    }
}
