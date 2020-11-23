using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services.Operations;
using Microsoft.AspNetCore.Mvc;

namespace OperationsExample.Controllers
{
    [Route("api/bulk")]
    public class OperationsController : JsonApiOperationsController
    {
        public OperationsController(IOperationsProcessor processor)
            : base(processor)
        {
        }
    }
}
