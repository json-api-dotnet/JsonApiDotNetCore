using System;
using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Microservices
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class DomainUser : Identifiable<Guid>
    {
        [Attr]
        [Required]
        public string LoginName { get; set; }

        [Attr]
        public string DisplayName { get; set; }

        [HasOne]
        public DomainGroup Group { get; set; }
    }
}
