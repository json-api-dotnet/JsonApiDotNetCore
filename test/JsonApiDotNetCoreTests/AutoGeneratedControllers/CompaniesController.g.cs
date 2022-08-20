using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.SoftDeletion;

namespace JsonApiDotNetCoreTests.IntegrationTests.SoftDeletion;

public sealed partial class CompaniesController : JsonApiController<Company, int>
{
    public CompaniesController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<Company, int> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
