using JsonApiDotNetCore.Controllers.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace JsonApiDotNetCoreTests.IntegrationTests.MultiTenancy
{
    [DisableRoutingConvention]
    [Route("{countryCode}/products")]
    partial class WebProductsController
    {
    }
}
