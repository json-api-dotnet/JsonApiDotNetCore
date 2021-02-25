using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExample.Controllers
{
    public sealed class AuthorsController : JsonApiController<Author>
    {
        public AuthorsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Author> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
