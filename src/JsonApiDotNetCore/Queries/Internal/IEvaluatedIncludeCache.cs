using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Queries.Internal
{
    /// <summary>
    /// Provides in-memory storage for the evaluated inclusion tree within a request. This tree is produced from query string and resource definition
    /// callbacks. The cache enables the serialization layer to take changes from <see cref="IResourceDefinition{TResource,TId}.OnApplyIncludes" /> into
    /// account.
    /// </summary>
    public interface IEvaluatedIncludeCache
    {
        /// <summary>
        /// Stores the evaluated inclusion tree for later usage.
        /// </summary>
        void Set(IncludeExpression include);

        /// <summary>
        /// Gets the evaluated inclusion tree that was stored earlier.
        /// </summary>
        IncludeExpression? Get();
    }
}
