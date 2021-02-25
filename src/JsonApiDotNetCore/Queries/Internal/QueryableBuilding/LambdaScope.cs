using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries.Internal.QueryableBuilding
{
    /// <summary>
    /// Contains details on a lambda expression, such as the name of the selector "x" in "x => x.Name".
    /// </summary>
    [PublicAPI]
    public sealed class LambdaScope : IDisposable
    {
        private readonly LambdaParameterNameScope _parameterNameScope;

        public ParameterExpression Parameter { get; }
        public Expression Accessor { get; }
        public HasManyThroughAttribute HasManyThrough { get; }

        public LambdaScope(LambdaParameterNameFactory nameFactory, Type elementType, Expression accessorExpression, HasManyThroughAttribute hasManyThrough)
        {
            ArgumentGuard.NotNull(nameFactory, nameof(nameFactory));
            ArgumentGuard.NotNull(elementType, nameof(elementType));

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
