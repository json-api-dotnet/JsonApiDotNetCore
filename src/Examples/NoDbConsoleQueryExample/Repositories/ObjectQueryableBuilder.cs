using System.Linq.Expressions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.Internal.QueryableBuilding;
using JsonApiDotNetCore.Resources;
using Microsoft.EntityFrameworkCore.Metadata;

namespace NoDbConsoleQueryExample.Repositories;

[PublicAPI]
public class ObjectQueryableBuilder : QueryableBuilder
{
    public ObjectQueryableBuilder(Expression source, Type elementType, Type extensionType, LambdaParameterNameFactory nameFactory,
        IResourceFactory resourceFactory, IModel entityModel, LambdaScopeFactory? lambdaScopeFactory = null)
        : base(source, elementType, extensionType, nameFactory, resourceFactory, entityModel, lambdaScopeFactory)
    {
    }

    protected override Expression ApplyInclude(Expression source, IncludeExpression include, ResourceType resourceType)
    {
        // This prevents emitting a call to 'Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.Include()',
        // which is unavailable when not using Entity Framework Core.
        // But since we have all data in memory, there's no need for it anyway.
        return source;
    }
}
