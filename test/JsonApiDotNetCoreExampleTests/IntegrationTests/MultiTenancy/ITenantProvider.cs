using System;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.MultiTenancy
{
    public interface ITenantProvider
    {
        Guid TenantId { get; }
    }
}
