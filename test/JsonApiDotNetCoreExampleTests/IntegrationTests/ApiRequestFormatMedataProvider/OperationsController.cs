// using System.Collections.Generic;
// using System.Threading;
// using System.Threading.Tasks;
// using JsonApiDotNetCore.AtomicOperations;
// using JsonApiDotNetCore.Configuration;
// using JsonApiDotNetCore.Controllers;
// using JsonApiDotNetCore.Middleware;
// using JsonApiDotNetCore.Resources;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.Extensions.Logging;
//
// namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ApiRequestFormatMedataProvider
// {
//     public sealed class OperationsController : JsonApiOperationsController
//     {
//         public OperationsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IOperationsProcessor processor, IJsonApiRequest request,
//             ITargetedFields targetedFields)
//             : base(options, loggerFactory, processor, request, targetedFields)
//         {
//         }
//
//         [Consumes("application/vnd.api+json;ext=\"https://jsonapi.org/ext/atomic\"")]
//         [HttpPost]
//         public override Task<IActionResult> PostOperationsAsync(IList<OperationContainer> operations, CancellationToken cancellationToken)
//         {
//             return base.PostOperationsAsync(operations, cancellationToken);
//         }
//     }
// }
