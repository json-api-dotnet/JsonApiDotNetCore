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
    /// Implements the foundational ASP.NET controller layer in the JsonApiDotNetCore architecture for handling atomic:operations requests. See
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
            var requestModelState = new List<(string key, ModelStateEntry? entry)>();
            int maxErrorsRemaining = ModelState.MaxAllowedErrors;

            foreach (OperationContainer operation in operations)
            {
                if (maxErrorsRemaining < 1)
                {
                    break;
                }

                maxErrorsRemaining = ValidateOperation(operation, operationIndex, requestModelState, maxErrorsRemaining);

                operationIndex++;
            }

            if (requestModelState.Any())
            {
                Dictionary<string, ModelStateEntry?> modelStateDictionary = requestModelState.ToDictionary(tuple => tuple.key, tuple => tuple.entry);

                throw new InvalidModelStateException(modelStateDictionary, typeof(IList<OperationContainer>), _options.IncludeExceptionStackTraceInErrors,
                    _resourceGraph,
                    (collectionType, index) => collectionType == typeof(IList<OperationContainer>) ? operations[index].Resource.GetType() : null);
            }
        }

        private int ValidateOperation(OperationContainer operation, int operationIndex, List<(string key, ModelStateEntry? entry)> requestModelState,
            int maxErrorsRemaining)
        {
            if (operation.Request.WriteOperation is WriteOperationKind.CreateResource or WriteOperationKind.UpdateResource)
            {
                _targetedFields.CopyFrom(operation.TargetedFields);
                _request.CopyFrom(operation.Request);

                var validationContext = new ActionContext
                {
                    ModelState =
                    {
                        MaxAllowedErrors = maxErrorsRemaining
                    }
                };

                ObjectValidator.Validate(validationContext, null, string.Empty, operation.Resource);

                if (!validationContext.ModelState.IsValid)
                {
                    int errorsRemaining = maxErrorsRemaining;

                    foreach (string key in validationContext.ModelState.Keys)
                    {
                        ModelStateEntry entry = validationContext.ModelState[key]!;

                        if (entry.ValidationState == ModelValidationState.Invalid)
                        {
                            string operationKey = $"[{operationIndex}].{nameof(OperationContainer.Resource)}.{key}";

                            if (entry.Errors.Count > 0 && entry.Errors[0].Exception is TooManyModelErrorsException)
                            {
                                requestModelState.Insert(0, (operationKey, entry));
                            }
                            else
                            {
                                requestModelState.Add((operationKey, entry));
                            }

                            errorsRemaining -= entry.Errors.Count;
                        }
                    }

                    return errorsRemaining;
                }
            }

            return maxErrorsRemaining;
        }
    }
}
