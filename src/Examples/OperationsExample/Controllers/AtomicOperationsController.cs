using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Controllers.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace OperationsExample.Controllers
{
    [DisableRoutingConvention, Route("api/v1/operations")]
    public class AtomicOperationsController : JsonApiAtomicOperationsController
    {
        public AtomicOperationsController(IJsonApiOptions options, ILoggerFactory loggerFactory,
            IAtomicOperationsProcessor processor)
            : base(options, loggerFactory, processor)
        {
        }
    }
}
