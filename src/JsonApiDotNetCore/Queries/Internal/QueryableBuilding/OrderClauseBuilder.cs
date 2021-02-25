using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries.Internal.QueryableBuilding
{
    /// <summary>
    /// Transforms <see cref="SortExpression" /> into
    /// <see cref="Queryable.OrderBy{TSource, TKey}(IQueryable{TSource}, System.Linq.Expressions.Expression{System.Func{TSource,TKey}})" /> calls.
    /// </summary>
    [PublicAPI]
    public class OrderClauseBuilder : QueryClauseBuilder<Expression>
    {
        private readonly Expression _source;
        private readonly Type _extensionType;

        public OrderClauseBuilder(Expression source, LambdaScope lambdaScope, Type extensionType)
            : base(lambdaScope)
        {
            ArgumentGuard.NotNull(source, nameof(source));
            ArgumentGuard.NotNull(extensionType, nameof(extensionType));

            _source = source;
            _extensionType = extensionType;
        }

        public Expression ApplyOrderBy(SortExpression expression)
        {
            ArgumentGuard.NotNull(expression, nameof(expression));

            return Visit(expression, null);
        }

        public override Expression VisitSort(SortExpression expression, Expression argument)
        {
            Expression sortExpression = null;

            foreach (SortElementExpression sortElement in expression.Elements)
            {
                sortExpression = Visit(sortElement, sortExpression);
            }

            return sortExpression;
        }

        public override Expression VisitSortElement(SortElementExpression expression, Expression previousExpression)
        {
            Expression body = expression.Count != null ? Visit(expression.Count, null) : Visit(expression.TargetAttribute, null);

            LambdaExpression lambda = Expression.Lambda(body, LambdaScope.Parameter);

            string operationName = GetOperationName(previousExpression != null, expression.IsAscending);

            return ExtensionMethodCall(previousExpression ?? _source, operationName, body.Type, lambda);
        }

        private static string GetOperationName(bool hasPrecedingSort, bool isAscending)
        {
            if (hasPrecedingSort)
            {
                return isAscending ? "ThenBy" : "ThenByDescending";
            }

            return isAscending ? "OrderBy" : "OrderByDescending";
        }

        private Expression ExtensionMethodCall(Expression source, string operationName, Type keyType, LambdaExpression keySelector)
        {
            Type[] typeArguments = ArrayFactory.Create(LambdaScope.Parameter.Type, keyType);
            return Expression.Call(_extensionType, operationName, typeArguments, source, keySelector);
        }

        protected override MemberExpression CreatePropertyExpressionForFieldChain(IReadOnlyCollection<ResourceFieldAttribute> chain, Expression source)
        {
            string[] components = chain.Select(GetPropertyName).ToArray();
            return CreatePropertyExpressionFromComponents(LambdaScope.Accessor, components);
        }

        private static string GetPropertyName(ResourceFieldAttribute field)
        {
            // In case of a HasManyThrough access (from count() function), we only need to look at the number of entries in the join table.
            return field is HasManyThroughAttribute hasManyThrough ? hasManyThrough.ThroughProperty.Name : field.Property.Name;
        }
    }
}
