using JsonApiDotNetCore.Controllers.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace DatabasePerTenantExample.Controllers;

// Workaround for https://youtrack.jetbrains.com/issue/RSRP-487028
public partial class EmployeesController
{
}

[DisableRoutingConvention]
[Route("api/{tenantName}/employees")]
partial class EmployeesController
{
}
