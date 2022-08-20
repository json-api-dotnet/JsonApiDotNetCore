using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;

namespace JsonApiDotNetCoreExample.Controllers;

public sealed partial class PeopleController : JsonApiController<Person, int>
{
    public PeopleController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<Person, int> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
