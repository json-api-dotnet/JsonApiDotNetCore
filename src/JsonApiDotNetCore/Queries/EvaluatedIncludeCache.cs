using JsonApiDotNetCore.Queries.Expressions;

namespace JsonApiDotNetCore.Queries;

/// <inheritdoc cref="IEvaluatedIncludeCache" />
internal sealed class EvaluatedIncludeCache : IEvaluatedIncludeCache
{
    private readonly IQueryConstraintProvider[] _constraintProviders;
    private IncludeExpression? _include;
    private bool _isAssigned;

    public EvaluatedIncludeCache(IEnumerable<IQueryConstraintProvider> constraintProviders)
    {
        ArgumentNullException.ThrowIfNull(constraintProviders);

        _constraintProviders = constraintProviders as IQueryConstraintProvider[] ?? constraintProviders.ToArray();
    }

    /// <inheritdoc />
    public void Set(IncludeExpression include)
    {
        ArgumentNullException.ThrowIfNull(include);

        _include = include;
        _isAssigned = true;
    }

    /// <inheritdoc />
    public IncludeExpression? Get()
    {
        if (!_isAssigned)
        {
            // In case someone has replaced the built-in JsonApiResourceService with their own that "forgets" to populate the cache,
            // then as a fallback, we feed the requested includes from query string to the response serializer.

            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:wrap_before_first_method_call true

            _include = _constraintProviders
                .SelectMany(provider => provider.GetConstraints())
                .Where(constraint => constraint.Scope == null)
                .Select(constraint => constraint.Expression)
                .OfType<IncludeExpression>()
                .FirstOrDefault();

            // @formatter:wrap_before_first_method_call restore
            // @formatter:wrap_chained_method_calls restore
            _isAssigned = true;
        }

        return _include;
    }
}
