using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;
using SourceGeneratorDebugger.Models;

namespace SourceGeneratorDebugger.Controllers
{
    public sealed class CustomersController : JsonApiController<Customer, long>
    {
        public CustomersController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
            IResourceService<Customer, long> resourceService)
            : base(options, resourceGraph, loggerFactory, resourceService)
        {
        }
    }
}
