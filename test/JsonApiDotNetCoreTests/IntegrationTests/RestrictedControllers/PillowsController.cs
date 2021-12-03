using JsonApiDotNetCore.Controllers.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.RestrictedControllers
{
    // Workaround for https://youtrack.jetbrains.com/issue/RSRP-487028
    public partial class PillowsController
    {
    }

    [DisableQueryString("skipCache")]
    partial class PillowsController
    {
    }
}
