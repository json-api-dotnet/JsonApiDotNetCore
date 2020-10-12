using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExample.Controllers
{
    public sealed class ProductsController : JsonApiController<Product>
    {
        public ProductsController(
            IJsonApiOptions options,
            ILoggerFactory loggerFactory,
            IResourceService<Product> resourceService)
            : base(options, loggerFactory, resourceService)
        { }
    }
}
