#nullable disable

using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Serialization
{
    public sealed class StudentsController : JsonApiController<Student, int>
    {
        public StudentsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Student, int> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
