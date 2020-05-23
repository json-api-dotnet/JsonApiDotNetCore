using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExample.Controllers
{
    public sealed class CategoriesController: JsonApiController<Category>
    {
        public CategoriesController(
            IJsonApiOptions jsonApiOptions,
            ILoggerFactory loggerFactory,
            IResourceService<Category> resourceService)
            : base(jsonApiOptions, loggerFactory, resourceService)
        { }
    }
}
