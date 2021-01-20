using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Controllers.Annotations;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.AtomicOperations.Controllers
{
    [DisableRoutingConvention, Route("/operations/musicTracks/create")]
    public sealed class CreateMusicTrackOperationsController : JsonApiOperationsController
    {
        public CreateMusicTrackOperationsController(IJsonApiOptions options, ILoggerFactory loggerFactory,
            IOperationsProcessor processor, IJsonApiRequest request, ITargetedFields targetedFields)
            : base(options, loggerFactory, processor, request, targetedFields)
        {
        }

        public override async Task<IActionResult> PostOperationsAsync(IList<OperationContainer> operations, CancellationToken cancellationToken)
        {
            AssertOnlyCreatingMusicTracks(operations);

            return await base.PostOperationsAsync(operations, cancellationToken);
        }

        private static void AssertOnlyCreatingMusicTracks(IEnumerable<OperationContainer> operations)
        {
            int index = 0;
            foreach (var operation in operations)
            {
                if (operation.Kind != OperationKind.CreateResource || operation.Resource.GetType() != typeof(MusicTrack))
                {
                    throw new JsonApiException(new Error(HttpStatusCode.UnprocessableEntity)
                    {
                        Title = "Unsupported combination of operation code and resource type at this endpoint.",
                        Detail = "This endpoint can only be used to create resources of type 'musicTracks'.",
                        Source =
                        {
                            Pointer = $"/atomic:operations[{index}]"
                        }
                    });
                }

                index++;
            }
        }
    }
}
