using System.Net;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ControllerActionResults
{
    public abstract class BaseToothbrushesController : BaseJsonApiController<Toothbrush>
    {
        protected BaseToothbrushesController(IJsonApiOptions options, ILoggerFactory loggerFactory,
            IResourceService<Toothbrush> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }

        public override async Task<IActionResult> GetAsync(int id, CancellationToken cancellationToken)
        {
            if (id == 11111111)
            {
                return NotFound();
            }

            if (id == 22222222)
            {
                return NotFound(new Error(HttpStatusCode.NotFound)
                {
                    Title = "No toothbrush with that ID exists."
                });
            }

            if (id == 33333333)
            {
                return Conflict("Something went wrong.");
            }

            if (id == 44444444)
            {
                return Error(new Error(HttpStatusCode.BadGateway));
            }

            if (id == 55555555)
            {
                var errors = new[]
                {
                    new Error(HttpStatusCode.PreconditionFailed),
                    new Error(HttpStatusCode.Unauthorized),
                    new Error(HttpStatusCode.ExpectationFailed)
                    {
                        Title = "This is not a very great request."
                    }
                };
                return Error(errors);
            }

            return await base.GetAsync(id, cancellationToken);
        }
    }
}