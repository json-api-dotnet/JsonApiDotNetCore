using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Serialization.Objects
{
    /// <summary>
    /// A meta object containing non-standard meta-information about the error.
    /// </summary>
    [PublicAPI]
    public sealed class ErrorMeta
    {
        [JsonExtensionData]
        public IDictionary<string, object> Data { get; } = new Dictionary<string, object>();

        public void IncludeExceptionStackTrace(Exception exception)
        {
            if (exception == null)
            {
                Data.Remove("StackTrace");
            }
            else
            {
                Data["StackTrace"] = exception.ToString().Split("\n", int.MaxValue, StringSplitOptions.RemoveEmptyEntries);
            }
        }
    }
}
