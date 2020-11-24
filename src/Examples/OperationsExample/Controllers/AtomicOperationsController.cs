using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace OperationsExample.Controllers
{
    [Route("api/v1/operations")]
    public class AtomicOperationsController : JsonApiAtomicOperationsController
    {
        public AtomicOperationsController(IAtomicOperationsProcessor processor)
            : base(processor)
        {
        }
    }
}
