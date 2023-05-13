using JsonApiDotNetCore.Queries.Expressions;

namespace JsonApiDotNetCore.Queries.Internal;

/// <inheritdoc />
internal sealed class EvaluatedIncludeCache : IEvaluatedIncludeCache
{
    private readonly IEnumerable<IQueryConstraintProvider> _constraintProviders;
    private IncludeExpression? _include;
    private bool _isAssigned;

    public EvaluatedIncludeCache(IEnumerable<IQueryConstraintProvider> constraintProviders)
    {
        ArgumentGuard.NotNull(constraintProviders);

        _constraintProviders = constraintProviders;
    }

    /// <inheritdoc />
    public void Set(IncludeExpression include)
    {
        ArgumentGuard.NotNull(include);

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
            // @formatter:keep_existing_linebreaks true

            _include = _constraintProviders.SelectMany(provider => provider.GetConstraints())
                .Where(constraint => constraint.Scope == null)
                .Select(constraint => constraint.Expression)
                .OfType<IncludeExpression>()
                .FirstOrDefault();

            // @formatter:keep_existing_linebreaks restore
            // @formatter:wrap_chained_method_calls restore
            _isAssigned = true;
        }

        return _include;
    }
}
