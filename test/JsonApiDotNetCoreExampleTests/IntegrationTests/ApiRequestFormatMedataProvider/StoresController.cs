using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ApiRequestFormatMedataProvider
{
    public sealed class StoresController : JsonApiController<Store>
    {
        public StoresController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Store> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }

        [Consumes("application/vnd.api+json")]
        [HttpPost]
        public override async Task<IActionResult> PostAsync([FromBody] Store resource, CancellationToken cancellationToken)
        {
            return await base.PostAsync(resource, cancellationToken);
        }
    }
}
