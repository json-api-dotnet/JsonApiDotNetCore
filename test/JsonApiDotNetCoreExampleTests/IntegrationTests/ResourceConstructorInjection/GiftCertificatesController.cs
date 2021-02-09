using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceConstructorInjection
{
    public sealed class GiftCertificatesController : JsonApiController<GiftCertificate>
    {
        public GiftCertificatesController(IJsonApiOptions options, ILoggerFactory loggerFactory,
            IResourceService<GiftCertificate> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
