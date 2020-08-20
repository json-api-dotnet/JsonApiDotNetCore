using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExample.Controllers
{
    [DisableQueryString("skipCache")]
    public sealed class TagsController : JsonApiController<Tag>
    {
        public TagsController(
            IJsonApiOptions options,
            ILoggerFactory loggerFactory,
            IResourceService<Tag, int> resourceService)
            : base(options, loggerFactory, resourceService)
        { }
    }
}
