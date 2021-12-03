using System.Linq.Expressions;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.Queries.Internal.QueryableBuilding
{
    [PublicAPI]
    public sealed class LambdaScopeFactory
    {
        private readonly LambdaParameterNameFactory _nameFactory;

        public LambdaScopeFactory(LambdaParameterNameFactory nameFactory)
        {
            ArgumentGuard.NotNull(nameFactory, nameof(nameFactory));

            _nameFactory = nameFactory;
        }

        public LambdaScope CreateScope(Type elementType, Expression? accessorExpression = null)
        {
            ArgumentGuard.NotNull(elementType, nameof(elementType));

            return new LambdaScope(_nameFactory, elementType, accessorExpression);
        }
    }
}
