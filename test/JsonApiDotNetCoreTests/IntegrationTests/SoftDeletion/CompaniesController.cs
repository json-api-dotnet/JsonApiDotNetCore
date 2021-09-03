using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.SoftDeletion
{
    public sealed class CompaniesController : JsonApiController<Company>
    {
        public CompaniesController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Company> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
