using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Serialization;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Serialization;

public sealed partial class ScholarshipsController : JsonApiController<Scholarship, int>
{
    public ScholarshipsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<Scholarship, int> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
