using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.AtomicOperations;

/// <summary>
/// Determines whether an operation in an atomic:operations request can be used. For non-operations requests, see <see cref="IJsonApiEndpointFilter" />.
/// </summary>
/// <remarks>
/// The default implementation relies on the usage of <see cref="ResourceAttribute.GenerateControllerEndpoints" />. If you're using explicit
/// (non-generated) controllers, register your own implementation to indicate which operations are accessible.
/// </remarks>
[PublicAPI]
public interface IAtomicOperationFilter
{
    /// <summary>
    /// An <see cref="IAtomicOperationFilter" /> that always returns <c>true</c>. Provided for convenience, to revert to the original behavior from before
    /// filtering was introduced.
    /// </summary>
    public static IAtomicOperationFilter AlwaysEnabled { get; } = new AlwaysEnabledOperationFilter();

    /// <summary>
    /// Determines whether the specified operation can be used in an atomic:operations request.
    /// </summary>
    /// <param name="resourceType">
    /// The targeted primary resource type of the operation.
    /// </param>
    /// <param name="writeOperation">
    /// The operation kind.
    /// </param>
    bool IsEnabled(ResourceType resourceType, WriteOperationKind writeOperation);

    private sealed class AlwaysEnabledOperationFilter : IAtomicOperationFilter
    {
        public bool IsEnabled(ResourceType resourceType, WriteOperationKind writeOperation)
        {
            return true;
        }
    }
}
