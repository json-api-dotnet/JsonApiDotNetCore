using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceInheritance
{
    public sealed class StudentsController : JsonApiController<Student>
    {
        public StudentsController(IJsonApiOptions options, ILoggerFactory loggerFactory,
            IResourceService<Student> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
