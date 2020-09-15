using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ModelStateValidation
{
    public sealed class PostalAddressesController : JsonApiController<PostalAddress>
    {
        public PostalAddressesController(IJsonApiOptions options, ILoggerFactory loggerFactory,
            IResourceService<PostalAddress> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
