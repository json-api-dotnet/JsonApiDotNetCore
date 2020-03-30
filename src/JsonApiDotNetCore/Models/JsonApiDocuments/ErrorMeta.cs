using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Models.JsonApiDocuments
{
    public sealed class ErrorMeta
    {
        [JsonProperty("stackTrace")]
        public ICollection<string> StackTrace { get; set; }

        public static ErrorMeta FromException(Exception e)
            => new ErrorMeta {
                StackTrace = e.Demystify().ToString().Split(new[] { "\n"}, int.MaxValue, StringSplitOptions.RemoveEmptyEntries)
            };
    }
}
