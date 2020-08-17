using System;
using System.Linq.Expressions;
using JsonApiDotNetCore.Models.Annotation;

namespace JsonApiDotNetCore.Internal.Queries.QueryableBuilding
{
    public sealed class LambdaScopeFactory
    {
        private readonly LambdaParameterNameFactory _nameFactory;
        private readonly HasManyThroughAttribute _hasManyThrough;

        public LambdaScopeFactory(LambdaParameterNameFactory nameFactory, HasManyThroughAttribute hasManyThrough = null)
        {
            _nameFactory = nameFactory ?? throw new ArgumentNullException(nameof(nameFactory));
            _hasManyThrough = hasManyThrough;
        }

        public LambdaScope CreateScope(Type elementType, Expression accessorExpression = null)
        {
            if (elementType == null)
            {
                throw new ArgumentNullException(nameof(elementType));
            }

            return new LambdaScope(_nameFactory, elementType, accessorExpression, _hasManyThrough);
        }
    }
}
