#nullable disable

using System;

namespace JsonApiDotNetCoreTests.IntegrationTests.MultiTenancy
{
    public interface ITenantProvider
    {
        // An implementation would obtain the tenant ID from the request, for example from the incoming
        // authentication token, a custom HTTP header, the route or a query string parameter.
        Guid TenantId { get; }
    }
}
