using System;
using System.Collections.Generic;

namespace JsonApiDotNetCore.Serialization
{
    internal static class MetaExtensions
    {
        public static void IncludeExceptionStackTrace(this IDictionary<string, object> meta, Exception exception)
        {
            ArgumentGuard.NotNull(exception, nameof(exception));

            meta["StackTrace"] = exception.ToString().Split("\n", int.MaxValue, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
