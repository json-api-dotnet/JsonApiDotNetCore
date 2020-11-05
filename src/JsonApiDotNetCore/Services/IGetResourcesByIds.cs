using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Services
{
    /// <summary>
    /// Gets resources by set of identifiers for a type that is known at runtime.
    /// </summary>
    // TODO: Refactor this type (it is a helper method).
    public interface IGetResourcesByIds
    {
        /// <summary>
        /// Retrieves resources of type <paramref name="resourceType"/> where the identifiers match <paramref name="typedIds"/>. 
        /// </summary>
        /// <param name="resourceType">The resource type to get.</param>
        /// <param name="typedIds">The identifiers of the resources to get.</param>
        /// <returns></returns>
        Task<IReadOnlyCollection<IIdentifiable>> Get(Type resourceType, ISet<object> typedIds);
    }
}
