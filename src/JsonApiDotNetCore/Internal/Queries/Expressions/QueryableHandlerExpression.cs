using System;
using System.Linq;
using JsonApiDotNetCore.Models;
using Microsoft.Extensions.Primitives;

namespace JsonApiDotNetCore.Internal.Queries.Expressions
{
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
