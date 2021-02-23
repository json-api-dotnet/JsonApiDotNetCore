using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.SoftDeletion
{
    public sealed class DepartmentsController : JsonApiController<Department>
    {
        public DepartmentsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Department> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
