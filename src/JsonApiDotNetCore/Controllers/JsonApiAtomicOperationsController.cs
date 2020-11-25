using System;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.AspNetCore.Mvc;

namespace JsonApiDotNetCore.Controllers
{
    /// <summary>
    /// A controller to be used for bulk operations as defined in the json:api 1.1 specification
    /// </summary>
    public class JsonApiAtomicOperationsController : ControllerBase
    {
        private readonly IAtomicOperationsProcessor _atomicOperationsProcessor;

        /// <param name="atomicOperationsProcessor">
        /// The processor to handle bulk operations.
        /// </param>
        public JsonApiAtomicOperationsController(IAtomicOperationsProcessor atomicOperationsProcessor)
        {
            _atomicOperationsProcessor = atomicOperationsProcessor ?? throw new ArgumentNullException(nameof(atomicOperationsProcessor));
        }

        /// <summary>
        /// Bulk endpoint for json:api operations
        /// </summary>
        /// <param name="doc">
        /// A json:api operations request document
        /// </param>
        /// <param name="cancellationToken">Propagates notification that request handling should be canceled.</param>
        /// <example>
        /// <code>
        /// POST /api/v1/operations HTTP/1.1
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
        [HttpPost]
        public virtual async Task<IActionResult> PostOperationsAsync([FromBody] AtomicOperationsDocument doc, CancellationToken cancellationToken)
        {
            if (doc == null) return new StatusCodeResult(422);

            var results = await _atomicOperationsProcessor.ProcessAsync(doc.Operations, cancellationToken);

            return Ok(new AtomicOperationsDocument
            {
                Operations = results
            });
        }
    }
}
