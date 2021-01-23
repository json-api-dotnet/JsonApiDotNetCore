using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Controllers
{
    /// <summary>
    /// Implements the foundational ASP.NET Core controller layer in the JsonApiDotNetCore architecture for handling atomic:operations requests.
    /// See https://jsonapi.org/ext/atomic/ for details. Delegates work to <see cref="IOperationsProcessor"/>.
    /// </summary>
    public abstract class BaseJsonApiOperationsController : CoreJsonApiController
    {
        private readonly IJsonApiOptions _options;
        private readonly IOperationsProcessor _processor;
        private readonly IJsonApiRequest _request;
        private readonly ITargetedFields _targetedFields;
        private readonly TraceLogWriter<BaseJsonApiOperationsController> _traceWriter;

        protected BaseJsonApiOperationsController(IJsonApiOptions options, ILoggerFactory loggerFactory,
            IOperationsProcessor processor, IJsonApiRequest request, ITargetedFields targetedFields)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));

            _options = options ?? throw new ArgumentNullException(nameof(options));
            _processor = processor ?? throw new ArgumentNullException(nameof(processor));
            _request = request ?? throw new ArgumentNullException(nameof(request));
            _targetedFields = targetedFields ?? throw new ArgumentNullException(nameof(targetedFields));
            _traceWriter = new TraceLogWriter<BaseJsonApiOperationsController>(loggerFactory);
        }

        /// <summary>
        /// Atomically processes a list of operations and returns a list of results.
        /// All changes are reverted if processing fails.
        /// If processing succeeds but none of the operations returns any data, then HTTP 201 is returned instead of 200.
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
        /// ]]></code></example>
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
        /// ]]></code></example>
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
        /// ]]></code></example>
        public virtual async Task<IActionResult> PostOperationsAsync([FromBody] IList<OperationContainer> operations,
            CancellationToken cancellationToken)
        {
            _traceWriter.LogMethodStart(new {operations});
            if (operations == null) throw new ArgumentNullException(nameof(operations));

            ValidateClientGeneratedIds(operations);

            if (_options.ValidateModelState)
            {
                ValidateModelState(operations);
            }

            var results = await _processor.ProcessAsync(operations, cancellationToken);
            return results.Any(result => result != null) ? (IActionResult) Ok(results) : NoContent();
        }

        protected virtual void ValidateClientGeneratedIds(IEnumerable<OperationContainer> operations)
        {
            if (!_options.AllowClientGeneratedIds)
            {
                int index = 0;
                foreach (var operation in operations)
                {
                    if (operation.Kind == OperationKind.CreateResource && operation.Resource.StringId != null)
                    {
                        throw new ResourceIdInCreateResourceNotAllowedException(index);
                    }

                    index++;
                }
            }
        }

        protected virtual void ValidateModelState(IEnumerable<OperationContainer> operations)
        {
            // We must validate the resource inside each operation manually, because they are typed as IIdentifiable.
            // Instead of validating IIdentifiable we need to validate the resource runtime-type.

            var violations = new List<ModelStateViolation>();

            int index = 0;
            foreach (var operation in operations)
            {
                if (operation.Kind == OperationKind.CreateResource || operation.Kind == OperationKind.UpdateResource)
                {
                    _targetedFields.Attributes = operation.TargetedFields.Attributes;
                    _targetedFields.Relationships = operation.TargetedFields.Relationships;

                    _request.CopyFrom(operation.Request);

                    var validationContext = new ActionContext();
                    ObjectValidator.Validate(validationContext, null, string.Empty, operation.Resource);

                    if (!validationContext.ModelState.IsValid)
                    {
                        foreach (var (key, entry) in validationContext.ModelState)
                        {
                            foreach (var error in entry.Errors)
                            {
                                var violation = new ModelStateViolation($"/atomic:operations[{index}]/data/attributes/", key, operation.Resource.GetType(), error);
                                violations.Add(violation);
                            }
                        }
                    }
                }

                index++;
            }

            if (violations.Any())
            {
                var namingStrategy = _options.SerializerContractResolver.NamingStrategy;
                throw new InvalidModelStateException(violations, _options.IncludeExceptionStackTraceInErrors, namingStrategy);
            }
        }
    }
}
