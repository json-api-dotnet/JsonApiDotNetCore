#nullable disable

using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Controllers.Annotations;
using JsonApiDotNetCore.QueryStrings;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.RestrictedControllers
{
    [NoHttpDelete]
    [DisableQueryString(JsonApiQueryStringParameters.Sort | JsonApiQueryStringParameters.Page)]
    public sealed class BlockingHttpDeleteController : JsonApiController<Sofa, int>
    {
        public BlockingHttpDeleteController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
            IResourceService<Sofa, int> resourceService)
            : base(options, resourceGraph, loggerFactory, resourceService)
        {
        }
    }
}
