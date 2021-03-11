using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries.Internal.QueryableBuilding
{
    /// <summary>
    /// Base class for transforming <see cref="QueryExpression" /> trees into system <see cref="Expression" /> trees.
    /// </summary>
    public abstract class QueryClauseBuilder<TArgument> : QueryExpressionVisitor<TArgument, Expression>
    {
        protected LambdaScope LambdaScope { get; }

        protected QueryClauseBuilder(LambdaScope lambdaScope)
        {
            ArgumentGuard.NotNull(lambdaScope, nameof(lambdaScope));

            LambdaScope = lambdaScope;
        }

        public override Expression VisitCount(CountExpression expression, TArgument argument)
        {
            Expression collectionExpression = Visit(expression.TargetCollection, argument);

            Expression propertyExpression = TryGetCollectionCount(collectionExpression);

            if (propertyExpression == null)
            {
                throw new InvalidOperationException($"Field '{expression.TargetCollection}' must be a collection.");
            }

            return propertyExpression;
        }

        private static Expression TryGetCollectionCount(Expression collectionExpression)
        {
            var properties = new HashSet<PropertyInfo>(collectionExpression.Type.GetProperties());

            if (collectionExpression.Type.IsInterface)
            {
                foreach (PropertyInfo item in collectionExpression.Type.GetInterfaces().SelectMany(@interface => @interface.GetProperties()))
                {
                    properties.Add(item);
                }
            }

            foreach (PropertyInfo property in properties)
            {
                if (property.Name == "Count" || property.Name == "Length")
                {
                    return Expression.Property(collectionExpression, property);
                }
            }

            return null;
        }

        public override Expression VisitResourceFieldChain(ResourceFieldChainExpression expression, TArgument argument)
        {
            return CreatePropertyExpressionForFieldChain(expression.Fields, LambdaScope.Accessor);
        }

        protected virtual MemberExpression CreatePropertyExpressionForFieldChain(IReadOnlyCollection<ResourceFieldAttribute> chain, Expression source)
        {
            string[] components = chain.Select(field => field is RelationshipAttribute relationship ? relationship.RelationshipPath : field.Property.Name)
                .ToArray();

            return CreatePropertyExpressionFromComponents(source, components);
        }

        protected static MemberExpression CreatePropertyExpressionFromComponents(Expression source, IReadOnlyCollection<string> components)
        {
            MemberExpression property = null;

            foreach (string propertyName in components)
            {
                Type parentType = property == null ? source.Type : property.Type;

                if (parentType.GetProperty(propertyName) == null)
                {
                    throw new InvalidOperationException($"Type '{parentType.Name}' does not contain a property named '{propertyName}'.");
                }

                property = property == null ? Expression.Property(source, propertyName) : Expression.Property(property, propertyName);
            }

            return property;
        }

        protected Expression CreateTupleAccessExpressionForConstant(object value, Type type)
        {
            // To enable efficient query plan caching, inline constants (that vary per request) should be converted into query parameters.
            // https://stackoverflow.com/questions/54075758/building-a-parameterized-entityframework-core-expression

            // This method can be used to change a query like:
            //   SELECT ... FROM ... WHERE x."Age" = 3
            // into:
            //   SELECT ... FROM ... WHERE x."Age" = @p0

            // The code below builds the next expression for a type T that is unknown at compile time:
            //   Expression.Property(Expression.Constant(Tuple.Create<T>(value)), "Item1")
            // Which represents the next C# code:
            //   Tuple.Create<T>(value).Item1;

            MethodInfo tupleCreateMethod = typeof(Tuple).GetMethods()
                .Single(method => method.Name == "Create" && method.IsGenericMethod && method.GetGenericArguments().Length == 1);

            MethodInfo constructedTupleCreateMethod = tupleCreateMethod.MakeGenericMethod(type);

            ConstantExpression constantExpression = Expression.Constant(value, type);

            MethodCallExpression tupleCreateCall = Expression.Call(constructedTupleCreateMethod, constantExpression);
            return Expression.Property(tupleCreateCall, "Item1");
        }
    }
}
