using System;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.MultiTenancy
{
    internal sealed class FakeTenantProvider : ITenantProvider
    {
        public Guid TenantId { get; }

        public FakeTenantProvider(Guid tenantId)
        {
            // A real implementation would be registered at request scope and obtain the tenant ID from the request, for example
            // from the incoming authentication token, a custom HTTP header, the route or a query string parameter.

            TenantId = tenantId;
        }
    }
}
