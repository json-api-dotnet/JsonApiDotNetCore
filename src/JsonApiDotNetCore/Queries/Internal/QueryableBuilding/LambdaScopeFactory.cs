using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries.Internal.QueryableBuilding
{
    [PublicAPI]
    public sealed class LambdaScopeFactory
    {
        private readonly LambdaParameterNameFactory _nameFactory;
        private readonly HasManyThroughAttribute _hasManyThrough;

        public LambdaScopeFactory(LambdaParameterNameFactory nameFactory, HasManyThroughAttribute hasManyThrough = null)
        {
            ArgumentGuard.NotNull(nameFactory, nameof(nameFactory));

            _nameFactory = nameFactory;
            _hasManyThrough = hasManyThrough;
        }

        public LambdaScope CreateScope(Type elementType, Expression accessorExpression = null)
        {
            ArgumentGuard.NotNull(elementType, nameof(elementType));

            return new LambdaScope(_nameFactory, elementType, accessorExpression, _hasManyThrough);
        }
    }
}
