using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models.Annotation;
using JsonApiDotNetCore.Queries.Expressions;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCore.Internal.Queries.QueryableBuilding
{
    /// <summary>
    /// Transforms <see cref="IncludeExpression"/> into <see cref="EntityFrameworkQueryableExtensions.Include{TEntity, TProperty}"/> calls.
    /// </summary>
    public class IncludeClauseBuilder : QueryClauseBuilder<object>
    {
        private readonly Expression _source;
        private readonly ResourceContext _resourceContext;
        private readonly IResourceContextProvider _resourceContextProvider;

        public IncludeClauseBuilder(Expression source, LambdaScope lambdaScope, ResourceContext resourceContext,
            IResourceContextProvider resourceContextProvider)
            : base(lambdaScope)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _resourceContext = resourceContext ?? throw new ArgumentNullException(nameof(resourceContext));
            _resourceContextProvider = resourceContextProvider ?? throw new ArgumentNullException(nameof(resourceContextProvider));
        }

        public Expression ApplyInclude(IncludeExpression include)
        {
            if (include == null)
            {
                throw new ArgumentNullException(nameof(include));
            }

            return Visit(include, null);
        }

        public override Expression VisitInclude(IncludeExpression expression, object argument)
        {
            var source = ApplyEagerLoads(_source, _resourceContext.EagerLoads, null);

            foreach (ResourceFieldChainExpression chain in IncludeChainConverter.GetRelationshipChains(expression))
            {
                string path = null;

                foreach (var relationship in chain.Fields.Cast<RelationshipAttribute>())
                {
                    path = path == null ? relationship.RelationshipPath : path + "." + relationship.RelationshipPath;

                    var resourceContext = _resourceContextProvider.GetResourceContext(relationship.RightType);
                    source = ApplyEagerLoads(source, resourceContext.EagerLoads, path);
                }

                source = IncludeExtensionMethodCall(source, path);
            }

            return source;
        }

        private Expression ApplyEagerLoads(Expression source, IEnumerable<EagerLoadAttribute> eagerLoads, string pathPrefix)
        {
            foreach (var eagerLoad in eagerLoads)
            {
                string path = pathPrefix != null ? pathPrefix + "." + eagerLoad.Property.Name : eagerLoad.Property.Name;
                source = IncludeExtensionMethodCall(source, path);

                source = ApplyEagerLoads(source, eagerLoad.Children, path);
            }

            return source;
        }

        private Expression IncludeExtensionMethodCall(Expression source, string navigationPropertyPath)
        {
            Expression navigationExpression = Expression.Constant(navigationPropertyPath);

            return Expression.Call(typeof(EntityFrameworkQueryableExtensions), "Include", new[]
            {
                LambdaScope.Parameter.Type
            }, source, navigationExpression);
        }
    }
}
