using System;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ZeroKeys
{
    public sealed class MapsController : JsonApiController<Map, Guid?>
    {
        public MapsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Map, Guid?> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
