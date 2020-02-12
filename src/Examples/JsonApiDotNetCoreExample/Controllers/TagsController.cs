using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExample.Controllers
{
    public class TagsController : JsonApiController<Tag>
    {
        public TagsController(
            IJsonApiOptions jsonApiOptions,
            IResourceService<Tag> resourceService,
            ILoggerFactory loggerFactory) 
            : base(jsonApiOptions, resourceService, loggerFactory)
        { }
    }
}
