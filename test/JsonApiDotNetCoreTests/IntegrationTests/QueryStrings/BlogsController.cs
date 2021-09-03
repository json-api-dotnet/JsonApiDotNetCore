using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings
{
    public sealed class BlogsController : JsonApiController<Blog>
    {
        public BlogsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Blog> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
