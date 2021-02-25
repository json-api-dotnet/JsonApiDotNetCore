using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Hooks.Internal.Execution
{
    /// <summary>
    /// A wrapper that contains a resource that is affected by the request, matched to its current database value
    /// </summary>
    [PublicAPI]
    public sealed class ResourceDiffPair<TResource>
        where TResource : class, IIdentifiable
    {
        /// <summary>
        /// The resource from the request matching the resource from the database.
        /// </summary>
        public TResource Resource { get; }

        /// <summary>
        /// The resource from the database matching the resource from the request.
        /// </summary>
        public TResource DatabaseValue { get; }

        public ResourceDiffPair(TResource resource, TResource databaseValue)
        {
            Resource = resource;
            DatabaseValue = databaseValue;
        }
    }
}
