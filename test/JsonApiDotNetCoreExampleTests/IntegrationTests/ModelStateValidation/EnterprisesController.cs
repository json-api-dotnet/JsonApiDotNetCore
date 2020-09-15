using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ModelStateValidation
{
    public sealed class EnterprisesController : JsonApiController<Enterprise>
    {
        public EnterprisesController(IJsonApiOptions options, ILoggerFactory loggerFactory,
            IResourceService<Enterprise> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
