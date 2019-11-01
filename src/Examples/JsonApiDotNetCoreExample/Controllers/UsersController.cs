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
            IResourceService<User> resourceService,
            ILoggerFactory loggerFactory) 
            : base(jsonApiOptions, resourceService, loggerFactory)
        { }
    }

    public class SuperUsersContrroller : JsonApiController<SuperUser>
    {
        public SuperUsersContrroller(
            IJsonApiOptions jsonApiOptions,
            IResourceService<SuperUser> resourceService,
            ILoggerFactory loggerFactory)
            : base(jsonApiOptions, resourceService, loggerFactory)
        { }
    }
}
