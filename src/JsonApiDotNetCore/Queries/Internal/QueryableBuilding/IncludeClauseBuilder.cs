using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCore.Queries.Internal.QueryableBuilding
{
    /// <summary>
    /// Transforms <see cref="IncludeExpression" /> into <see cref="EntityFrameworkQueryableExtensions.Include{TEntity, TProperty}" /> calls.
    /// </summary>
    [PublicAPI]
    public class IncludeClauseBuilder : QueryClauseBuilder<object?>
    {
        private static readonly IncludeChainConverter IncludeChainConverter = new();

        private readonly Expression _source;
        private readonly ResourceType _resourceType;

        public IncludeClauseBuilder(Expression source, LambdaScope lambdaScope, ResourceType resourceType)
            : base(lambdaScope)
        {
            ArgumentGuard.NotNull(source, nameof(source));
            ArgumentGuard.NotNull(resourceType, nameof(resourceType));

            _source = source;
            _resourceType = resourceType;
        }

        public Expression ApplyInclude(IncludeExpression include)
        {
            ArgumentGuard.NotNull(include, nameof(include));

            return Visit(include, null);
        }

        public override Expression VisitInclude(IncludeExpression expression, object? argument)
        {
            Expression source = ApplyEagerLoads(_source, _resourceType.EagerLoads, null);

            foreach (ResourceFieldChainExpression chain in IncludeChainConverter.GetRelationshipChains(expression))
            {
                source = ProcessRelationshipChain(chain, source);
            }

            return source;
        }

        private Expression ProcessRelationshipChain(ResourceFieldChainExpression chain, Expression source)
        {
            string? path = null;
            Expression result = source;

            foreach (RelationshipAttribute relationship in chain.Fields.Cast<RelationshipAttribute>())
            {
                path = path == null ? relationship.Property.Name : $"{path}.{relationship.Property.Name}";

                result = ApplyEagerLoads(result, relationship.RightType.EagerLoads, path);
            }

            return IncludeExtensionMethodCall(result, path!);
        }

        private Expression ApplyEagerLoads(Expression source, IEnumerable<EagerLoadAttribute> eagerLoads, string? pathPrefix)
        {
            Expression result = source;

            foreach (EagerLoadAttribute eagerLoad in eagerLoads)
            {
                string path = pathPrefix != null ? $"{pathPrefix}.{eagerLoad.Property.Name}" : eagerLoad.Property.Name;
                result = IncludeExtensionMethodCall(result, path);

                result = ApplyEagerLoads(result, eagerLoad.Children, path);
            }

            return result;
        }

        private Expression IncludeExtensionMethodCall(Expression source, string navigationPropertyPath)
        {
            Expression navigationExpression = Expression.Constant(navigationPropertyPath);

            return Expression.Call(typeof(EntityFrameworkQueryableExtensions), "Include", LambdaScope.Parameter.Type.AsArray(), source, navigationExpression);
        }
    }
}
