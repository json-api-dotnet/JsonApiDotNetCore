using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExample.Controllers
{
    public class UsersController : JsonApiController<User>
    {
        public UsersController(IJsonApiOptions jsonApiOptions, ILoggerFactory loggerFactory,
            IResourceService<User> resourceService)
            : base(jsonApiOptions, loggerFactory, resourceService)
        {
        }
    }

    public class SuperUsersController : JsonApiController<SuperUser>
    {
        public SuperUsersController(IJsonApiOptions jsonApiOptions, ILoggerFactory loggerFactory,
            IResourceService<SuperUser> resourceService)
            : base(jsonApiOptions, loggerFactory, resourceService)
        {
        }
    }
}
