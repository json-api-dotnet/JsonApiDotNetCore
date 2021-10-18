using System;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.ZeroKeys
{
    public sealed class MapsController : JsonApiController<Map, Guid?>
    {
        public MapsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory, IResourceService<Map, Guid?> resourceService)
            : base(options, resourceGraph, loggerFactory, resourceService)
        {
        }
    }
}
