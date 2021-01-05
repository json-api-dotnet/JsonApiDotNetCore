using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Controllers
{
    /// <summary>
    /// The base class to derive atomic:operations controllers from.
    /// This class delegates all work to <see cref="BaseJsonApiAtomicOperationsController"/> but adds attributes for routing templates.
    /// If you want to provide routing templates yourself, you should derive from BaseJsonApiAtomicOperationsController directly.
    /// </summary>
    /// <remarks>
    /// Your project-specific controller should be decorated with the next attributes:
    /// <example><code><![CDATA[
    /// [DisableRoutingConvention, Route("api/v1/operations")]
    /// ]]></code></example>
    /// </remarks>
    public abstract class JsonApiAtomicOperationsController : BaseJsonApiAtomicOperationsController
    {
        protected JsonApiAtomicOperationsController(IJsonApiOptions options, ILoggerFactory loggerFactory,
            IAtomicOperationsProcessor processor, IJsonApiRequest request, ITargetedFields targetedFields)
            : base(options, loggerFactory, processor, request, targetedFields)
        {
        }

        /// <inheritdoc />
        [HttpPost]
        public override async Task<IActionResult> PostOperationsAsync([FromBody] IList<OperationContainer> operations,
            CancellationToken cancellationToken)
        {
            return await base.PostOperationsAsync(operations, cancellationToken);
        }
    }
}
