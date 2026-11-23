using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Reading;

/// <summary>
/// Decorates <see cref="SparseFieldSetEvaluator" /> to count calls, which proves that <see cref="ISparseFieldSetEvaluator" /> is the replaceable
/// extensibility point.
/// </summary>
[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class CountingSparseFieldSetEvaluator(
    IEnumerable<IQueryConstraintProvider> constraintProviders, IResourceDefinitionAccessor resourceDefinitionAccessor,
    SparseFieldSetEvaluatorHitCounter hitCounter) : ISparseFieldSetEvaluator
{
    private readonly SparseFieldSetEvaluator _inner = new(constraintProviders, resourceDefinitionAccessor);
    private readonly SparseFieldSetEvaluatorHitCounter _hitCounter = hitCounter;

    public SparseFieldSetExpression? GetSparseFieldSetForQuery(ResourceType resourceType)
    {
        _hitCounter.Calls.Add($"{resourceType.PublicName}:ForQuery");
        SparseFieldSetExpression? result = _inner.GetSparseFieldSetForQuery(resourceType);
        _hitCounter.QueryResults.Add($"{resourceType.PublicName}:{result?.ToString() ?? "null"}");
        return result;
    }

    public SparseFieldSetExpression GetSparseFieldSetForSerializer(ResourceType resourceType)
    {
        _hitCounter.Calls.Add($"{resourceType.PublicName}:ForSerializer");
        return _inner.GetSparseFieldSetForSerializer(resourceType);
    }

    public void Reset()
    {
        _hitCounter.Calls.Add("Reset");
        _inner.Reset();
    }
}
