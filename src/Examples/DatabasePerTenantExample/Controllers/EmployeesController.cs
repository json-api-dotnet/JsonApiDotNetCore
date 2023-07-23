using JsonApiDotNetCore.Controllers.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace DatabasePerTenantExample.Controllers;

[DisableRoutingConvention]
[Route("api/{tenantName}/employees")]
partial class EmployeesController
{
}
