using JsonApiDotNetCore.Controllers.Annotations;
using JsonApiDotNetCore.QueryStrings;

namespace JsonApiDotNetCoreTests.IntegrationTests.RestrictedControllers;

// Workaround for https://youtrack.jetbrains.com/issue/RSRP-487028
public partial class SofasController
{
}

[DisableQueryString(JsonApiQueryStringParameters.Sort | JsonApiQueryStringParameters.Page)]
partial class SofasController
{
}
