using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;

namespace JsonApiDotNetCoreExample.Controllers;

public sealed partial class TagsController : JsonApiController<Tag, int>
{
    public TagsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<Tag, int> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
