using System;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.AspNetCore.Authentication;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceConstructorInjection
{
    public sealed class GiftCertificate : Identifiable
    {
        private readonly ISystemClock _systemClock;

        [Attr]
        public DateTimeOffset IssueDate { get; set; }

        [Attr(Capabilities = AttrCapabilities.AllowView)]
        [NotMapped]
        public bool HasExpired => IssueDate.AddYears(1) < _systemClock.UtcNow;

        [HasOne]
        public PostOffice Issuer { get; set; }

        public GiftCertificate(InjectionDbContext injectionDbContext)
        {
            _systemClock = injectionDbContext.SystemClock;
        }
    }
}
