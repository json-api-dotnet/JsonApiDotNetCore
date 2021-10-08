using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.SoftDeletion
{
    public sealed class CompaniesController : JsonApiController<Company, int>
    {
        public CompaniesController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Company, int> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
