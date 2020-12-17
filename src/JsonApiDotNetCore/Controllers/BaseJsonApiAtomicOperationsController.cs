using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Controllers
{
    /// <summary>
    /// Implements the foundational ASP.NET Core controller layer in the JsonApiDotNetCore architecture for handling atomic:operations requests.
    /// See https://jsonapi.org/ext/atomic/ for details. Delegates work to <see cref="IAtomicOperationsProcessor"/>.
    /// </summary>
    public abstract class BaseJsonApiAtomicOperationsController : CoreJsonApiController
    {
        private readonly IJsonApiOptions _options;
        private readonly IAtomicOperationsProcessor _processor;
        private readonly TraceLogWriter<BaseJsonApiAtomicOperationsController> _traceWriter;

        protected BaseJsonApiAtomicOperationsController(IJsonApiOptions options, ILoggerFactory loggerFactory,
            IAtomicOperationsProcessor processor)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));

            _options = options ?? throw new ArgumentNullException(nameof(options));
            _processor = processor ?? throw new ArgumentNullException(nameof(processor));
            _traceWriter = new TraceLogWriter<BaseJsonApiAtomicOperationsController>(loggerFactory);
        }

        /// <summary>
        /// Atomically processes a document with operations and returns their results.
        /// If processing fails, all changes are reverted.
        /// If processing succeeds and none of the operations contain data, HTTP 201 is returned instead 200.
        /// </summary>
        /// <example>
        /// The next example creates a new resource.
        /// <code><![CDATA[
        /// POST /api/v1/operations HTTP/1.1
        /// Content-Type: application/vnd.api+json;ext="https://jsonapi.org/ext/atomic"
        /// 
        /// {
        ///   "atomic:operations": [{
        ///     "op": "add",
        ///     "data": {
        ///       "type": "authors",
        ///       "attributes": {
        ///         "name": "John Doe"
        ///       }
        ///     }
        ///   }]
        /// }
        /// ]]></code></example>
        /// <example>
        /// The next example updates an existing resource.
        /// <code><![CDATA[
        /// POST /api/v1/operations HTTP/1.1
        /// Content-Type: application/vnd.api+json;ext="https://jsonapi.org/ext/atomic"
        /// 
        /// {
        ///   "atomic:operations": [{
        ///     "op": "update",
        ///     "data": {
        ///       "type": "authors",
        ///       "id": 1,
        ///       "attributes": {
        ///         "name": "Jane Doe"
        ///       }
        ///     }
        ///   }]
        /// }
        /// ]]></code></example>
        /// <example>
        /// The next example deletes an existing resource.
        /// <code><![CDATA[
        /// POST /api/v1/operations HTTP/1.1
        /// Content-Type: application/vnd.api+json;ext="https://jsonapi.org/ext/atomic"
        /// 
        /// {
        ///   "atomic:operations": [{
        ///     "op": "remove",
        ///     "ref": {
        ///       "type": "authors",
        ///       "id": 1
        ///     }
        ///   }]
        /// }
        /// ]]></code></example>
        public virtual async Task<IActionResult> PostOperationsAsync([FromBody] AtomicOperationsDocument document,
            CancellationToken cancellationToken)
        {
            _traceWriter.LogMethodStart(new {document});

            if (document == null)
            {
                // TODO: @OPS: Should throw NullReferenceException here, but catch this error higher up the call stack (JsonApiReader).
                return new StatusCodeResult(422);
            }

            var results = await _processor.ProcessAsync(document.Operations, cancellationToken);

            if (results.Any(result => result.Data != null))
            {
                return Ok(new AtomicOperationsDocument
                {
                    Results = results
                });
            }

            return NoContent();
        }
    }
}
