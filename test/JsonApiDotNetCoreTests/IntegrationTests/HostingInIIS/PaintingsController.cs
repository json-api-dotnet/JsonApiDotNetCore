using JsonApiDotNetCore.Controllers.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace JsonApiDotNetCoreTests.IntegrationTests.HostingInIIS
{
    // Workaround for https://youtrack.jetbrains.com/issue/RSRP-487028
    public partial class PaintingsController
    {
    }

    [DisableRoutingConvention]
    [Route("custom/path/to/paintings-of-the-world")]
    partial class PaintingsController
    {
    }
}
