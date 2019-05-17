using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExample.Controllers
{
    public class UsersController : JsonApiController<User>
    {
        public UsersController(
            IJsonApiOptions jsonApiOptions,
            IJsonApiContext jsonApiContext,
            IResourceService<User> resourceService,
            ILoggerFactory loggerFactory) 
            : base(jsonApiOptions, jsonApiContext, resourceService, loggerFactory)
        { }
    }
}
