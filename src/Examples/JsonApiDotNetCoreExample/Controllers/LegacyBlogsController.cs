using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExample.Controllers
{
    public sealed class LegacyBlogsController : JsonApiController<LegacyBlog>
    {
        public LegacyBlogsController(
            IJsonApiOptions options,
            ILoggerFactory loggerFactory,
            IResourceService<LegacyBlog> resourceService)
            : base(options, loggerFactory, resourceService)
        { }
    }
}
