using System;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Annotation;

namespace JsonApiDotNetCoreExample.Models
{
    public sealed class Visa : Identifiable
    {
        [Attr]
        public DateTime ExpiresAt { get; set; }

        [Attr]
        public string CountryName => TargetCountry.Name;

        [EagerLoad]
        public Country TargetCountry { get; set; }
    }
}
