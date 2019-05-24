using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExample.Controllers
{
    public class UsersController : JsonApiController<User>
    {
        public UsersController(
            IJsonApiOptions jsonApiOptions,
            IResourceGraph resourceGraph,
            IResourceService<User> resourceService,
            ILoggerFactory loggerFactory) 
            : base(jsonApiOptions, resourceGraph, resourceService, loggerFactory)
        { }
    }
}
