using GettingStarted.Models;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace GettingStarted.Controllers
{
    public sealed class UserProfilesController : JsonApiController<UserProfile>
    {
        public UserProfilesController(IJsonApiOptions options, ILoggerFactory loggerFactory,
            IResourceService<UserProfile, int> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
