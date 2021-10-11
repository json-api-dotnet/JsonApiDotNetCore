#nullable disable

using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceConstructorInjection
{
    public sealed class GiftCertificatesController : JsonApiController<GiftCertificate, int>
    {
        public GiftCertificatesController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<GiftCertificate, int> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
