using System;
using System.Linq;
using JsonApiDotNetCore.Resources;
using Microsoft.Extensions.Primitives;

namespace JsonApiDotNetCore.Queries.Expressions
{
    /// <summary>
    /// Holds a <see cref="IQueryable{T}"/> expression, used for custom query string handlers from <see cref="ResourceDefinition{TResource}"/>s.
    /// </summary>
    public class QueryableHandlerExpression : QueryExpression
    {
        private readonly object _queryableHandler;
        private readonly StringValues _parameterValue;

        public QueryableHandlerExpression(object queryableHandler, StringValues parameterValue)
        {
            _queryableHandler = queryableHandler ?? throw new ArgumentNullException(nameof(queryableHandler));
            _parameterValue = parameterValue;
        }

        public IQueryable<TResource> Apply<TResource>(IQueryable<TResource> query)
            where TResource : class, IIdentifiable
        {
            var handler = (Func<IQueryable<TResource>, StringValues, IQueryable<TResource>>) _queryableHandler;
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
    }
}
