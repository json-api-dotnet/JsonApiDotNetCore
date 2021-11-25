using JsonApiDotNetCore.Controllers.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace JsonApiDotNetCoreTests.IntegrationTests.MultiTenancy
{
    // Workaround for https://youtrack.jetbrains.com/issue/RSRP-487028
    public partial class WebProductsController
    {
    }

    [DisableRoutingConvention]
    [Route("{countryCode}/products")]
    partial class WebProductsController
    {
    }
}
