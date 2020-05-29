using System;
using JsonApiDotNetCore.Models.Annotation;

namespace JsonApiDotNetCoreExample.Models
{
    public class Visa
    {
        public int Id { get; set; }

        public DateTime ExpiresAt { get; set; }

        [EagerLoad]
        public Country TargetCountry { get; set; }
    }
}
