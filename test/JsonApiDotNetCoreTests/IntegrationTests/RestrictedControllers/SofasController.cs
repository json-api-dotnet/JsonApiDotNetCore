using JsonApiDotNetCore.Controllers.Annotations;
using JsonApiDotNetCore.QueryStrings;

namespace JsonApiDotNetCoreTests.IntegrationTests.RestrictedControllers
{
    [DisableQueryString(JsonApiQueryStringParameters.Sort | JsonApiQueryStringParameters.Page)]
    partial class SofasController
    {
    }
}
