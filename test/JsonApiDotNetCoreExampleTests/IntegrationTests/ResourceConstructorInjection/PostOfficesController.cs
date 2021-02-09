using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceConstructorInjection
{
    public sealed class PostOfficesController : JsonApiController<PostOffice>
    {
        public PostOfficesController(IJsonApiOptions options, ILoggerFactory loggerFactory,
            IResourceService<PostOffice> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
