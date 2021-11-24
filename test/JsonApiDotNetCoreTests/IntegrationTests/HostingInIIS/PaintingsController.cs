using JsonApiDotNetCore.Controllers.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace JsonApiDotNetCoreTests.IntegrationTests.HostingInIIS
{
    [DisableRoutingConvention]
    [Route("custom/path/to/paintings-of-the-world")]
    partial class PaintingsController
    {
    }
}
