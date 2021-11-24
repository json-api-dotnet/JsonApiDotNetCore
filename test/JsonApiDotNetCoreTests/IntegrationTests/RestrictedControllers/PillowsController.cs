using JsonApiDotNetCore.Controllers.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.RestrictedControllers
{
    [DisableQueryString("skipCache")]
    partial class PillowsController
    {
    }
}
