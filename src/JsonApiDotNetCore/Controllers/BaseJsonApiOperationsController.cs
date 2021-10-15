using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Controllers
{
    /// <summary>
    /// Implements the foundational ASP.NET Core controller layer in the JsonApiDotNetCore architecture for handling atomic:operations requests. See
    /// https://jsonapi.org/ext/atomic/ for details. Delegates work to <see cref="IOperationsProcessor" />.
    /// </summary>
    [PublicAPI]
    public abstract class BaseJsonApiOperationsController : CoreJsonApiController
    {
        private readonly IJsonApiOptions _options;
        private readonly IResourceGraph _resourceGraph;
        private readonly IOperationsProcessor _processor;
        private readonly IJsonApiRequest _request;
        private readonly ITargetedFields _targetedFields;
        private readonly TraceLogWriter<BaseJsonApiOperationsController> _traceWriter;

        protected BaseJsonApiOperationsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
            IOperationsProcessor processor, IJsonApiRequest request, ITargetedFields targetedFields)
        {
            ArgumentGuard.NotNull(options, nameof(options));
            ArgumentGuard.NotNull(resourceGraph, nameof(resourceGraph));
            ArgumentGuard.NotNull(loggerFactory, nameof(loggerFactory));
            ArgumentGuard.NotNull(processor, nameof(processor));
            ArgumentGuard.NotNull(request, nameof(request));
            ArgumentGuard.NotNull(targetedFields, nameof(targetedFields));

            _options = options;
            _resourceGraph = resourceGraph;
            _processor = processor;
            _request = request;
            _targetedFields = targetedFields;
            _traceWriter = new TraceLogWriter<BaseJsonApiOperationsController>(loggerFactory);
        }

        /// <summary>
        /// Atomically processes a list of operations and returns a list of results. All changes are reverted if processing fails. If processing succeeds but
        /// none of the operations returns any data, then HTTP 201 is returned instead of 200.
        /// </summary>
        /// <example>
        /// The next example creates a new resource.
        /// <code><![CDATA[
        /// POST /operations HTTP/1.1
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
        /// ]]></code>
        /// </example>
        /// <example>
        /// The next example updates an existing resource.
        /// <code><![CDATA[
        /// POST /operations HTTP/1.1
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
        /// ]]></code>
        /// </example>
        /// <example>
        /// The next example deletes an existing resource.
        /// <code><![CDATA[
        /// POST /operations HTTP/1.1
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
        /// ]]></code>
        /// </example>
        public virtual async Task<IActionResult> PostOperationsAsync([FromBody] IList<OperationContainer> operations, CancellationToken cancellationToken)
        {
            _traceWriter.LogMethodStart(new
            {
                operations
            });

            ArgumentGuard.NotNull(operations, nameof(operations));

            if (_options.ValidateModelState)
            {
                ValidateModelState(operations);
            }

            IList<OperationContainer?> results = await _processor.ProcessAsync(operations, cancellationToken);
            return results.Any(result => result != null) ? Ok(results) : NoContent();
        }

        protected virtual void ValidateModelState(IList<OperationContainer> operations)
        {
            // We must validate the resource inside each operation manually, because they are typed as IIdentifiable.
            // Instead of validating IIdentifiable we need to validate the resource runtime-type.

            using IDisposable _ = new RevertRequestStateOnDispose(_request, _targetedFields);

            int operationIndex = 0;
            var requestModelState = new Dictionary<string, ModelStateEntry>();

            foreach (OperationContainer operation in operations)
            {
                if (operation.Request.WriteOperation is WriteOperationKind.CreateResource or WriteOperationKind.UpdateResource)
                {
                    _targetedFields.CopyFrom(operation.TargetedFields);
                    _request.CopyFrom(operation.Request);

                    var validationContext = new ActionContext();
                    ObjectValidator.Validate(validationContext, null, string.Empty, operation.Resource);

                    CopyValidationErrorsFromOperation(validationContext.ModelState, operationIndex, requestModelState);
                }

                operationIndex++;
            }

            if (requestModelState.Any())
            {
                throw new InvalidModelStateException(requestModelState, typeof(IList<OperationContainer>), _options.IncludeExceptionStackTraceInErrors,
                    _resourceGraph,
                    (collectionType, index) => collectionType == typeof(IList<OperationContainer>) ? operations[index].Resource.GetType() : null);
            }
        }

        private static void CopyValidationErrorsFromOperation(ModelStateDictionary operationModelState, int operationIndex,
            Dictionary<string, ModelStateEntry> requestModelState)
        {
            if (!operationModelState.IsValid)
            {
                foreach (string key in operationModelState.Keys)
                {
                    ModelStateEntry entry = operationModelState[key];

                    if (entry.ValidationState == ModelValidationState.Invalid)
                    {
                        string operationKey = $"[{operationIndex}].{nameof(OperationContainer.Resource)}." + key;
                        requestModelState[operationKey] = entry;
                    }
                }
            }
        }
    }
}
