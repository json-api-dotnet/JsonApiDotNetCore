using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;

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
    /// Gets the sparse fieldset to retrieve from the underlying data store, or null to retrieve all fields.
    /// </summary>
    SparseFieldSetExpression? GetSparseFieldSetForQuery(ResourceType resourceType);

    /// <summary>
    /// Gets the evaluated sparse fieldset to serialize into the response body.
    /// </summary>
    SparseFieldSetExpression GetSparseFieldSetForSerializer(ResourceType resourceType);

    /// <summary>
    /// Resets the cached results from resource definition callbacks.
    /// </summary>
    void Reset();
}
