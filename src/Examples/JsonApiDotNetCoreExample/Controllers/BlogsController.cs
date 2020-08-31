using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExample.Controllers
{
    public sealed class BlogsController : JsonApiController<Blog>
    {
        public BlogsController(
            IJsonApiOptions options,
            ILoggerFactory loggerFactory,
            IResourceService<Blog> resourceService)
            : base(options, loggerFactory, resourceService)
        { }
    }
}
