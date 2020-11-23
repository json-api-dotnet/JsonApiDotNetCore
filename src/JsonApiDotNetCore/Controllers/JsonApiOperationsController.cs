using System.Threading.Tasks;
using JsonApiDotNetCore.Models.Operations;
using JsonApiDotNetCore.Services.Operations;
using Microsoft.AspNetCore.Mvc;

namespace JsonApiDotNetCore.Controllers
{
    /// <summary>
    /// A controller to be used for bulk operations as defined in the json:api 1.1 specification
    /// </summary>
    public class JsonApiOperationsController : ControllerBase
    {
        private readonly IOperationsProcessor _operationsProcessor;

        /// <param name="operationsProcessor">
        /// The processor to handle bulk operations.
        /// </param>
        public JsonApiOperationsController(IOperationsProcessor operationsProcessor)
        {
            _operationsProcessor = operationsProcessor;
        }

        /// <summary>
        /// Bulk endpoint for json:api operations
        /// </summary>
        /// <param name="doc">
        /// A json:api operations request document
        /// </param>
        /// <example>
        /// <code>
        /// PATCH /api/bulk HTTP/1.1
        /// Content-Type: application/vnd.api+json
        /// 
        /// {
        ///   "operations": [{
        ///     "op": "add",
        ///     "ref": {
        ///       "type": "authors"
        ///     },
        ///     "data": {
        ///       "type": "authors",
        ///       "attributes": {
        ///         "name": "jaredcnance"
        ///       }
        ///     }
        ///   }]
        /// }
        /// </code>
        /// </example>
        [HttpPatch]
        public virtual async Task<IActionResult> PatchAsync([FromBody] OperationsDocument doc)
        {
            if (doc == null) return new StatusCodeResult(422);

            var results = await _operationsProcessor.ProcessAsync(doc.Operations);

            return Ok(new OperationsDocument(results));
        }
    }
}
