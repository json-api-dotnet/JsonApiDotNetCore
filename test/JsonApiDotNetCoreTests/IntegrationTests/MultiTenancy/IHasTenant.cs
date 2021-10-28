using System;
using JetBrains.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.MultiTenancy
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public interface IHasTenant
    {
        Guid TenantId { get; set; }
    }
}
