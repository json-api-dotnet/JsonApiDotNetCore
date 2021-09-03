using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.QueryStrings
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class WebAccount : Identifiable
    {
        [Attr]
        public string UserName { get; set; }

        [Attr(Capabilities = ~AttrCapabilities.AllowView)]
        public string Password { get; set; }

        [Attr]
        public string DisplayName { get; set; }

        [Attr(Capabilities = AttrCapabilities.All & ~(AttrCapabilities.AllowFilter | AttrCapabilities.AllowSort))]
        public DateTime? DateOfBirth { get; set; }

        [Attr]
        public string EmailAddress { get; set; }

        [HasMany]
        public IList<BlogPost> Posts { get; set; }

        [HasOne]
        public AccountPreferences Preferences { get; set; }
    }
}
