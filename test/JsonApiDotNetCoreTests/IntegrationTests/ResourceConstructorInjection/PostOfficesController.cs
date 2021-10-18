#nullable disable

using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceConstructorInjection
{
    public sealed class PostOfficesController : JsonApiController<PostOffice, int>
    {
        public PostOfficesController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
            IResourceService<PostOffice, int> resourceService)
            : base(options, resourceGraph, loggerFactory, resourceService)
        {
        }
    }
}
