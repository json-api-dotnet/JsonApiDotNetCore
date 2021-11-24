using JsonApiDotNetCore.Controllers.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace JsonApiDotNetCoreTests.IntegrationTests.MultiTenancy
{
    [DisableRoutingConvention]
    [Route("{countryCode}/shops")]
    partial class WebShopsController
    {
    }
}
