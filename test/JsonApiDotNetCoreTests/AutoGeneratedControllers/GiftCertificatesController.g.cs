using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.ResourceConstructorInjection;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceConstructorInjection;

public sealed partial class GiftCertificatesController : JsonApiController<GiftCertificate, int>
{
    public GiftCertificatesController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<GiftCertificate, int> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
