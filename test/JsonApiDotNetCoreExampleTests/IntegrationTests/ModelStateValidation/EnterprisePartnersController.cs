using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ModelStateValidation
{
    public sealed class EnterprisePartnersController : JsonApiController<EnterprisePartner>
    {
        public EnterprisePartnersController(IJsonApiOptions options, ILoggerFactory loggerFactory,
            IResourceService<EnterprisePartner> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
