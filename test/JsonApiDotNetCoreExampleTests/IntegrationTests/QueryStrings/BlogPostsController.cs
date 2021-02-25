using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.QueryStrings
{
    public sealed class BlogPostsController : JsonApiController<BlogPost>
    {
        public BlogPostsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<BlogPost> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
