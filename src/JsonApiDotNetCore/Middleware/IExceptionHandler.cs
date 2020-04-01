using System;
using JsonApiDotNetCore.Models.JsonApiDocuments;

namespace JsonApiDotNetCore.Middleware
{
    /// <summary>
    /// Central place to handle all exceptions. Log them and translate into Error response.
    /// </summary>
    public interface IExceptionHandler
    {
        ErrorDocument HandleException(Exception exception);
    }
}
