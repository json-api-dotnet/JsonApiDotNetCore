using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExample.Controllers
{
    [DisableQuery("skipCache")]
    public sealed class TagsController : JsonApiController<Tag>
    {
        public TagsController(
            IJsonApiOptions jsonApiOptions,
            ILoggerFactory loggerFactory,
            IResourceService<Tag, int> resourceService)
            : base(jsonApiOptions, loggerFactory, resourceService)
        { }
    }
}
