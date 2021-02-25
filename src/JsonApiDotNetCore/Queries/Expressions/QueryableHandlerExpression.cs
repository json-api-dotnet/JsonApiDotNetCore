using System;
using System.Linq;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using Microsoft.Extensions.Primitives;

namespace JsonApiDotNetCore.Queries.Expressions
{
    /// <summary>
    /// Holds an <see cref="IQueryable{T}" /> expression, used for custom query string handlers from <see cref="IResourceDefinition{TResource,TId}" />s.
    /// </summary>
    [PublicAPI]
    public class QueryableHandlerExpression : QueryExpression
    {
        private readonly object _queryableHandler;
        private readonly StringValues _parameterValue;

        public QueryableHandlerExpression(object queryableHandler, StringValues parameterValue)
        {
            ArgumentGuard.NotNull(queryableHandler, nameof(queryableHandler));

            _queryableHandler = queryableHandler;
            _parameterValue = parameterValue;
        }

        public IQueryable<TResource> Apply<TResource>(IQueryable<TResource> query)
            where TResource : class, IIdentifiable
        {
            var handler = (Func<IQueryable<TResource>, StringValues, IQueryable<TResource>>)_queryableHandler;
            return handler(query, _parameterValue);
        }

        public override TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument)
        {
            return visitor.VisitQueryableHandler(this, argument);
        }

        public override string ToString()
        {
            return $"handler('{_parameterValue}')";
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj is null || GetType() != obj.GetType())
            {
                return false;
            }

            var other = (QueryableHandlerExpression)obj;

            return _queryableHandler == other._queryableHandler && _parameterValue.Equals(other._parameterValue);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_queryableHandler, _parameterValue);
        }
    }
}
