using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Microservices
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    [Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.Microservices")]
    public sealed class DomainGroup : Identifiable<Guid>
    {
        [Attr]
        public string Name { get; set; } = null!;

        [HasMany]
        public ISet<DomainUser> Users { get; set; } = new HashSet<DomainUser>();
    }
}
