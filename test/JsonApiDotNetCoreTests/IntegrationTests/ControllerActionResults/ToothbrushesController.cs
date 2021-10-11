#nullable disable

using System.Net;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.ControllerActionResults
{
    public sealed class ToothbrushesController : BaseJsonApiController<Toothbrush, int>
    {
        internal const int EmptyActionResultId = 11111111;
        internal const int ActionResultWithErrorObjectId = 22222222;
        internal const int ActionResultWithStringParameter = 33333333;
        internal const int ObjectResultWithErrorObjectId = 44444444;
        internal const int ObjectResultWithErrorCollectionId = 55555555;

        public ToothbrushesController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Toothbrush, int> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }

        [HttpGet("{id}")]
        public override async Task<IActionResult> GetAsync(int id, CancellationToken cancellationToken)
        {
            if (id == EmptyActionResultId)
            {
                return NotFound();
            }

            if (id == ActionResultWithErrorObjectId)
            {
                return NotFound(new ErrorObject(HttpStatusCode.NotFound)
                {
                    Title = "No toothbrush with that ID exists."
                });
            }

            if (id == ActionResultWithStringParameter)
            {
                return Conflict("Something went wrong.");
            }

            if (id == ObjectResultWithErrorObjectId)
            {
                return Error(new ErrorObject(HttpStatusCode.BadGateway));
            }

            if (id == ObjectResultWithErrorCollectionId)
            {
                var errors = new[]
                {
                    new ErrorObject(HttpStatusCode.PreconditionFailed),
                    new ErrorObject(HttpStatusCode.Unauthorized),
                    new ErrorObject(HttpStatusCode.ExpectationFailed)
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
