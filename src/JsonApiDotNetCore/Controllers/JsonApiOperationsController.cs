using System.Threading.Tasks;
using JsonApiDotNetCore.Models.Operations;
using JsonApiDotNetCore.Services.Operations;
using Microsoft.AspNetCore.Mvc;

namespace JsonApiDotNetCore.Controllers
{
    public class JsonApiOperationsController : Controller
    {
        private readonly IOperationsProcessor _operationsProcessor;

        public JsonApiOperationsController(IOperationsProcessor operationsProcessor)
        {
            _operationsProcessor = operationsProcessor;
        }

        [HttpPatch]
        public async Task<IActionResult> PatchAsync(OperationsDocument doc)
        {
            var results = await _operationsProcessor.ProcessAsync(doc.Operations);
            return Ok(results);
        }
    }
}
