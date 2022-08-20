using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using DatabasePerTenantExample.Models;

namespace DatabasePerTenantExample.Controllers;

public sealed partial class EmployeesController : JsonApiController<Employee, System.Guid>
{
    public EmployeesController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<Employee, System.Guid> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
