using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Models.JsonApiDocuments
{
    /// <summary>
    /// A meta object containing non-standard meta-information about the error.
    /// </summary>
    public sealed class ErrorMeta
    {
        [JsonExtensionData]
        public Dictionary<string, object> Data { get; } = new Dictionary<string, object>();

        public void IncludeExceptionStackTrace(Exception exception)
        {
            if (exception == null)
            {
                Data.Remove("StackTrace");
            }
            else
            {
                Data["StackTrace"] = exception.Demystify().ToString()
                    .Split(new[] { "\n" }, int.MaxValue, StringSplitOptions.RemoveEmptyEntries);
            }
        }
    }
}
