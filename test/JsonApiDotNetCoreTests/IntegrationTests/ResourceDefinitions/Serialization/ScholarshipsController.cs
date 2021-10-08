using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Serialization
{
    public sealed class ScholarshipsController : JsonApiController<Scholarship, int>
    {
        public ScholarshipsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Scholarship> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
