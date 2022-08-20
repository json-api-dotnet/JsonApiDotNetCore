using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Serialization;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Serialization;

public sealed partial class StudentsController : JsonApiController<Student, int>
{
    public StudentsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<Student, int> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
