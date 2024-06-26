using System.Diagnostics.CodeAnalysis;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Services;

/// <summary />
public interface IUpdateService<TResource, in TId>
    where TResource : class, IIdentifiable<TId>
{
    /// <summary>
    /// Handles a JSON:API request to update the attributes and/or relationships of an existing resource. Only the values of sent attributes are replaced.
    /// And only the values of sent relationships are replaced.
    /// </summary>
    Task<TResource?> UpdateAsync([DisallowNull] TId id, TResource resource, CancellationToken cancellationToken);
}
