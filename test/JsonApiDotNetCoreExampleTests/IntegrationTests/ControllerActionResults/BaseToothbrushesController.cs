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
        internal const int EmptyActionResultId = 11111111;
        internal const int ActionResultWithErrorObjectId = 22222222;
        internal const int ActionResultWithStringParameter = 33333333;
        internal const int ObjectResultWithErrorObjectId = 44444444;
        internal const int ObjectResultWithErrorCollectionId = 55555555;

        protected BaseToothbrushesController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Toothbrush> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }

        public override async Task<IActionResult> GetAsync(int id, CancellationToken cancellationToken)
        {
            if (id == EmptyActionResultId)
            {
                return NotFound();
            }

            if (id == ActionResultWithErrorObjectId)
            {
                return NotFound(new Error(HttpStatusCode.NotFound)
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
                return Error(new Error(HttpStatusCode.BadGateway));
            }

            if (id == ObjectResultWithErrorCollectionId)
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
