using System.Collections.Immutable;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries;

/// <summary>
/// Takes sparse fieldsets from <see cref="IQueryConstraintProvider" />s and invokes
/// <see cref="IResourceDefinition{TResource,TId}.OnApplySparseFieldSet" /> on them.
/// </summary>
/// <remarks>
/// This service ensures that for each request (or operation per request), the resource definition callback is executed only twice per resource type. The
/// first invocation is used to obtain the fields to retrieve from the underlying data store, while the second invocation is used to determine which
/// fields to write to the response body.
/// </remarks>
public interface ISparseFieldSetEvaluator
{
    /// <summary>
    /// Gets the set of sparse fields to retrieve from the underlying data store. Returns an empty set to retrieve all fields.
    /// </summary>
    IImmutableSet<ResourceFieldAttribute> GetSparseFieldSetForQuery(ResourceType resourceType);

    /// <summary>
    /// Gets the evaluated set of sparse fields to serialize into the response body.
    /// </summary>
    IImmutableSet<ResourceFieldAttribute> GetSparseFieldSetForSerializer(ResourceType resourceType);

    /// <summary>
    /// Resets the cached results from resource definition callbacks.
    /// </summary>
    void Reset();
}
