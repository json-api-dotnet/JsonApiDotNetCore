using JsonApiDotNetCore.Queries.Expressions;

namespace JsonApiDotNetCore.Queries.Internal
{
    /// <inheritdoc />
    internal sealed class EvaluatedIncludeCache : IEvaluatedIncludeCache
    {
        private IncludeExpression _include;

        /// <inheritdoc />
        public void Set(IncludeExpression include)
        {
            _include = include;
        }

        /// <inheritdoc />
        public IncludeExpression Get()
        {
            return _include;
        }
    }
}
