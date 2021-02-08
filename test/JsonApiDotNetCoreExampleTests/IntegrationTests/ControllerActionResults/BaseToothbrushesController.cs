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
        public const int _emptyActionResultId = 11111111;
        public const int _actionResultWithErrorObjectId = 22222222;
        public const int _actionResultWithStringParameter = 33333333;
        public const int _objectResultWithErrorObjectId = 44444444;
        public const int _objectResultWithErrorCollectionId = 55555555;

        protected BaseToothbrushesController(IJsonApiOptions options, ILoggerFactory loggerFactory,
            IResourceService<Toothbrush> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }

        public override async Task<IActionResult> GetAsync(int id, CancellationToken cancellationToken)
        {
            if (id == _emptyActionResultId)
            {
                return NotFound();
            }

            if (id == _actionResultWithErrorObjectId)
            {
                return NotFound(new Error(HttpStatusCode.NotFound)
                {
                    Title = "No toothbrush with that ID exists."
                });
            }

            if (id == _actionResultWithStringParameter)
            {
                return Conflict("Something went wrong.");
            }

            if (id == _objectResultWithErrorObjectId)
            {
                return Error(new Error(HttpStatusCode.BadGateway));
            }

            if (id == _objectResultWithErrorCollectionId)
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
