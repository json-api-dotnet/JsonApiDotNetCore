using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings
{
    public sealed class BlogsController : JsonApiController<Blog, int>
    {
        public BlogsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Blog, int> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
