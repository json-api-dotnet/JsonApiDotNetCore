using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCoreTests.IntegrationTests.MultiTenancy;

internal sealed class RouteTenantProvider(IHttpContextAccessor httpContextAccessor) : ITenantProvider
{
    // In reality, this would be looked up in a database. We'll keep it hardcoded for simplicity.
    public static readonly IDictionary<string, Guid> TenantRegistry = new Dictionary<string, Guid>
    {
        ["nld"] = Guid.NewGuid(),
        ["ita"] = Guid.NewGuid()
    };

    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public Guid TenantId
    {
        get
        {
            if (_httpContextAccessor.HttpContext == null)
            {
                throw new InvalidOperationException();
            }

            string? countryCode = (string?)_httpContextAccessor.HttpContext.Request.RouteValues["countryCode"];
            return countryCode != null && TenantRegistry.TryGetValue(countryCode, out Guid tenantId) ? tenantId : Guid.Empty;
        }
    }
}
