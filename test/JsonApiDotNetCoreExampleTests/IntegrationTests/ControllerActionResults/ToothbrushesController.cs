using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ControllerActionResults
{
    public sealed class ToothbrushesController : BaseToothbrushesController
    {
        public ToothbrushesController(IJsonApiOptions options, ILoggerFactory loggerFactory,
            IResourceService<Toothbrush> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }

        [HttpGet("{id}")]
        public override Task<IActionResult> GetAsync(int id, CancellationToken cancellationToken)
        {
            return base.GetAsync(id, cancellationToken);
        }
    }
}
