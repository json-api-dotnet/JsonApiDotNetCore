using System;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

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
