using JsonApiDotNetCore.Controllers.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace JsonApiDotNetCoreTests.IntegrationTests.MultiTenancy;

// Workaround for https://youtrack.jetbrains.com/issue/RSRP-487028
public partial class WebShopsController
{
}

[DisableRoutingConvention]
[Route("{countryCode}/shops")]
partial class WebShopsController
{
}
