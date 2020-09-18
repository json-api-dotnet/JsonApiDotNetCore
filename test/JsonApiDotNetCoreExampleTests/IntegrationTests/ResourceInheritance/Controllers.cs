using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceInheritance
{
    public sealed class MalesController : JsonApiController<Male>
    {
        public MalesController(IJsonApiOptions options, ILoggerFactory loggerFactory,
            IResourceService<Male> resourceService)
            : base(options, loggerFactory, resourceService) { }
    }
    
    public sealed class FemalesController : JsonApiController<Female>
    {
        public FemalesController(IJsonApiOptions options, ILoggerFactory loggerFactory,
            IResourceService<Female> resourceService)
            : base(options, loggerFactory, resourceService) { }
    }
    
    public sealed class PlaceholdersController : JsonApiController<Placeholder>
    {
        public PlaceholdersController(IJsonApiOptions options, ILoggerFactory loggerFactory,
            IResourceService<Placeholder> resourceService)
            : base(options, loggerFactory, resourceService) { }
    }
}
