using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ContentNegotiation
{
    public sealed class PoliciesController : JsonApiController<Policy>
    {
        public PoliciesController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Policy> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
