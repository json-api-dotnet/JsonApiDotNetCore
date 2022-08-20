using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.SoftDeletion;

namespace JsonApiDotNetCoreTests.IntegrationTests.SoftDeletion;

public sealed partial class DepartmentsController : JsonApiController<Department, int>
{
    public DepartmentsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<Department, int> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
