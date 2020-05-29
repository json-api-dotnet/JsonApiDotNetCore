using System;
using System.Linq.Expressions;
using JsonApiDotNetCore.Models.Annotation;

namespace JsonApiDotNetCore.Internal.Queries.QueryableBuilding
{
    public sealed class LambdaScope : IDisposable
    {
        private readonly LambdaParameterNameScope _parameterNameScope;

        public ParameterExpression Parameter { get; }
        public Expression Accessor { get; }
        public HasManyThroughAttribute HasManyThrough { get; }

        public LambdaScope(LambdaParameterNameFactory nameFactory, Type elementType, Expression accessorExpression, HasManyThroughAttribute hasManyThrough)
        {
            _parameterNameScope = nameFactory.Create(elementType.Name);
            Parameter = Expression.Parameter(elementType, _parameterNameScope.Name);

            if (accessorExpression != null)
            {
                Accessor = accessorExpression;
            }
            else if (hasManyThrough != null)
            {
                Accessor = Expression.Property(Parameter, hasManyThrough.RightProperty);
            }
            else
            {
                Accessor = Parameter;
            }

            HasManyThrough = hasManyThrough;
        }

        public void Dispose()
        {
            _parameterNameScope.Dispose();
        }
    }
}
