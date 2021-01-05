using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Controllers.Annotations;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExample.Controllers
{
    [DisableRoutingConvention, Route("/api/v1/operations")]
    public class AtomicOperationsController : JsonApiAtomicOperationsController
    {
        public AtomicOperationsController(IJsonApiOptions options, ILoggerFactory loggerFactory,
            IAtomicOperationsProcessor processor, IJsonApiRequest request, ITargetedFields targetedFields)
            : base(options, loggerFactory, processor, request, targetedFields)
        {
        }
    }
}
