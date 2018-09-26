using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Query;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.Extensions
{
    // ReSharper disable once InconsistentNaming
    public static class IQueryableExtensions
    {
        private static MethodInfo _containsMethod;
        private static MethodInfo ContainsMethod
        {
            get
            {
                if (_containsMethod == null)
                {
                    _containsMethod = typeof(Enumerable)
                      .GetMethods(BindingFlags.Static | BindingFlags.Public)
                      .Where(m => m.Name == nameof(Enumerable.Contains) && m.GetParameters().Count() == 2)
                      .First();
                }
                return _containsMethod;
            }
        }

        public static IQueryable<TSource> Sort<TSource>(this IQueryable<TSource> source, IJsonApiContext jsonApiContext, List<SortQuery> sortQueries)
        {
            if (sortQueries == null || sortQueries.Count == 0)
                return source;

            var orderedEntities = source.Sort(jsonApiContext, sortQueries[0]);

            if (sortQueries.Count <= 1)
                return orderedEntities;

            for (var i = 1; i < sortQueries.Count; i++)
                orderedEntities = orderedEntities.Sort(jsonApiContext, sortQueries[i]);

            return orderedEntities;
        }

        public static IOrderedQueryable<TSource> Sort<TSource>(this IQueryable<TSource> source, IJsonApiContext jsonApiContext, SortQuery sortQuery)
        {
            if (sortQuery.IsAttributeOfRelationship)
            {
                var relatedAttrQuery = new RelatedAttrQuery(jsonApiContext, sortQuery);
                return sortQuery.Direction == SortDirection.Descending
                    ? source.OrderByDescending(relatedAttrQuery)
                    : source.OrderBy(relatedAttrQuery);
            }
            else
            {
                var attrQuery = new AttrQuery(jsonApiContext, sortQuery);
                return sortQuery.Direction == SortDirection.Descending
                    ? source.OrderByDescending(attrQuery)
                    : source.OrderBy(attrQuery);
            }
        }

        public static IOrderedQueryable<TSource> Sort<TSource>(this IOrderedQueryable<TSource> source, IJsonApiContext jsonApiContext, SortQuery sortQuery)
        {
            if (sortQuery.IsAttributeOfRelationship)
            {
                var relatedAttrQuery = new RelatedAttrQuery(jsonApiContext, sortQuery);
                return sortQuery.Direction == SortDirection.Descending
                    ? source.OrderByDescending(relatedAttrQuery)
                    : source.OrderBy(relatedAttrQuery);
            }
            else
            {
                var attrQuery = new AttrQuery(jsonApiContext, sortQuery);
                return sortQuery.Direction == SortDirection.Descending
                    ? source.OrderByDescending(attrQuery)
                    : source.OrderBy(attrQuery);
            }
        }

        public static IOrderedQueryable<TSource> OrderBy<TSource>(this IQueryable<TSource> source, BaseAttrQuery baseAttrQuery)
            => CallGenericOrderMethod(source, baseAttrQuery, "OrderBy");

        public static IOrderedQueryable<TSource> OrderByDescending<TSource>(this IQueryable<TSource> source, BaseAttrQuery baseAttrQuery)
            => CallGenericOrderMethod(source, baseAttrQuery, "OrderByDescending");

        public static IOrderedQueryable<TSource> ThenBy<TSource>(this IOrderedQueryable<TSource> source, BaseAttrQuery baseAttrQuery)
            => CallGenericOrderMethod(source, baseAttrQuery, "ThenBy");

        public static IOrderedQueryable<TSource> ThenByDescending<TSource>(this IOrderedQueryable<TSource> source, BaseAttrQuery baseAttrQuery)
            => CallGenericOrderMethod(source, baseAttrQuery, "ThenByDescending");

        public static IQueryable<TSource> Filter<TSource>(this IQueryable<TSource> source, IJsonApiContext jsonApiContext, FilterQuery filterQuery)
        {
            if (filterQuery == null)
                return source;

            if (filterQuery.IsAttributeOfRelationship)
                return source.Filter(new RelatedAttrQuery(jsonApiContext, filterQuery));

            return source.Filter(new AttrQuery(jsonApiContext, filterQuery));
        }

        public static IQueryable<TSource> Filter<TSource>(this IQueryable<TSource> source, BaseAttrQuery filterQuery)
        {
            if (filterQuery == null)
                return source;

            if (filterQuery.FilterOperation == FilterOperations.@in || filterQuery.FilterOperation == FilterOperations.nin)
                return CallGenericWhereContainsMethod(source,filterQuery);
            else
                return CallGenericWhereMethod(source, filterQuery);
        }

        private static Expression GetFilterExpressionLambda(Expression left, Expression right, FilterOperations operation)
        {
            Expression body;
            switch (operation)
            {
                case FilterOperations.eq:
                    // {model.Id == 1}
                    body = Expression.Equal(left, right);
                    break;
                case FilterOperations.lt:
                    // {model.Id < 1}
                    body = Expression.LessThan(left, right);
                    break;
                case FilterOperations.gt:
                    // {model.Id > 1}
                    body = Expression.GreaterThan(left, right);
                    break;
                case FilterOperations.le:
                    // {model.Id <= 1}
                    body = Expression.LessThanOrEqual(left, right);
                    break;
                case FilterOperations.ge:
                    // {model.Id >= 1}
                    body = Expression.GreaterThanOrEqual(left, right);
                    break;
                case FilterOperations.like:
                    body = Expression.Call(left, "Contains", null, right);
                    break;
                    // {model.Id != 1}
                case FilterOperations.ne:
                    body = Expression.NotEqual(left, right);
                    break;
                case FilterOperations.isnotnull:
                    // {model.Id != null}
                    body = Expression.NotEqual(left, right);
                    break;
                case FilterOperations.isnull:
                    // {model.Id == null}
                    body = Expression.Equal(left, right);
                    break;
                default:
                    throw new JsonApiException(500, $"Unknown filter operation {operation}");
            }

            return body;
        }

        public static IQueryable<TSource> Select<TSource>(this IQueryable<TSource> source, List<string> columns)
        {
            if (columns == null || columns.Count == 0)
                return source;

            var sourceType = source.ElementType;

            var resultType = typeof(TSource);

            // {model}
            var parameter = Expression.Parameter(sourceType, "model");

            var bindings = columns.Select(column => Expression.Bind(
                resultType.GetProperty(column), Expression.PropertyOrField(parameter, column)));

            // { new Model () { Property = model.Property } }
            var body = Expression.MemberInit(Expression.New(resultType), bindings);

            // { model => new TodoItem() { Property = model.Property } }
            var selector = Expression.Lambda(body, parameter);

            return source.Provider.CreateQuery<TSource>(
                Expression.Call(typeof(Queryable), "Select", new[] { sourceType, resultType },
                source.Expression, Expression.Quote(selector)));
        }

        public static IQueryable<T> PageForward<T>(this IQueryable<T> source, int pageSize, int pageNumber)
        {
            if (pageSize > 0)
            {
                if (pageNumber == 0)
                    pageNumber = 1;

                if (pageNumber > 0)
                    return source
                        .Skip((pageNumber - 1) * pageSize)
                        .Take(pageSize);
            }

            return source;
        }

        #region Generic method calls

        private static IOrderedQueryable<TSource> CallGenericOrderMethod<TSource>(IQueryable<TSource> source, BaseAttrQuery baseAttrQuery, string method)
        {
            // {x}
            var parameter = Expression.Parameter(typeof(TSource), "x");
            MemberExpression member;
            // {x.relationship.propertyName}
            if (baseAttrQuery.IsAttributeOfRelationship)
            {
                var relation = Expression.PropertyOrField(parameter, baseAttrQuery.RelationshipAttribute.InternalRelationshipName);
                member = Expression.Property(relation, baseAttrQuery.Attribute.InternalAttributeName);
            }
            // {x.propertyName}
            else
                member = Expression.Property(parameter, baseAttrQuery.Attribute.InternalAttributeName);

            // {x=>x.propertyName} or {x=>x.relationship.propertyName}
            var lambda = Expression.Lambda(member, parameter);

            // REFLECTION: source.OrderBy(x => x.Property)
            var orderByMethod = typeof(Queryable).GetMethods().First(x => x.Name == method && x.GetParameters().Length == 2);
            var orderByGeneric = orderByMethod.MakeGenericMethod(typeof(TSource), member.Type);
            var result = orderByGeneric.Invoke(null, new object[] { source, lambda });

            return (IOrderedQueryable<TSource>)result;
        }

        private static IQueryable<TSource> CallGenericWhereMethod<TSource>(IQueryable<TSource> source, BaseAttrQuery filter)
        {
            var op = filter.FilterOperation;
            var concreteType = typeof(TSource);
            PropertyInfo relationProperty = null;
            PropertyInfo property = null;
            MemberExpression left;
            ConstantExpression right;

            // {model}
            var parameter = Expression.Parameter(concreteType, "model");
            // Is relationship attribute
            if (filter.IsAttributeOfRelationship)
            {
                relationProperty = concreteType.GetProperty(filter.RelationshipAttribute.InternalRelationshipName);
                if (relationProperty == null)
                    throw new ArgumentException($"'{filter.RelationshipAttribute.InternalRelationshipName}' is not a valid relationship of '{concreteType}'");

                var relatedType = filter.RelationshipAttribute.Type;
                property = relatedType.GetProperty(filter.Attribute.InternalAttributeName);
                if (property == null)
                    throw new ArgumentException($"'{filter.Attribute.InternalAttributeName}' is not a valid attribute of '{filter.RelationshipAttribute.InternalRelationshipName}'");

                var leftRelationship = Expression.PropertyOrField(parameter, filter.RelationshipAttribute.InternalRelationshipName);
                // {model.Relationship}
                left = Expression.PropertyOrField(leftRelationship, property.Name);
            }
            // Is standalone attribute
            else
            {
                property = concreteType.GetProperty(filter.Attribute.InternalAttributeName);
                if (property == null)
                    throw new ArgumentException($"'{filter.Attribute.InternalAttributeName}' is not a valid property of '{concreteType}'");

                // {model.Id}
                left = Expression.PropertyOrField(parameter, property.Name);
            }

            try
            {
                if (op == FilterOperations.isnotnull || op == FilterOperations.isnull)
                    right = Expression.Constant(null);
                else
                {
                    // convert the incoming value to the target value type
                    // "1" -> 1
                    var convertedValue = TypeHelper.ConvertType(filter.PropertyValue, property.PropertyType);
                    // {1}
                    right = Expression.Constant(convertedValue, property.PropertyType);
                }

                var body = GetFilterExpressionLambda(left, right, filter.FilterOperation);
                var lambda = Expression.Lambda<Func<TSource, bool>>(body, parameter);

                return source.Where(lambda);
            }
            catch (FormatException)
            {
                throw new JsonApiException(400, $"Could not cast {filter.PropertyValue} to {property.PropertyType.Name}");
            }
        }

        private static IQueryable<TSource> CallGenericWhereContainsMethod<TSource>(IQueryable<TSource> source, BaseAttrQuery filter)
        {
            var concreteType = typeof(TSource);
            var property = concreteType.GetProperty(filter.Attribute.InternalAttributeName);

            try
            {
                var propertyValues = filter.PropertyValue.Split(QueryConstants.COMMA);
                ParameterExpression entity = Expression.Parameter(concreteType, "entity");
                MemberExpression member;
                if (filter.IsAttributeOfRelationship)
                {
                    var relation = Expression.PropertyOrField(entity, filter.RelationshipAttribute.InternalRelationshipName);
                    member = Expression.Property(relation, filter.Attribute.InternalAttributeName);
                }
                else
                    member = Expression.Property(entity, filter.Attribute.InternalAttributeName);

                var method = ContainsMethod.MakeGenericMethod(member.Type);
                var obj = TypeHelper.ConvertListType(propertyValues, member.Type);

                if (filter.FilterOperation == FilterOperations.@in)
                {
                    // Where(i => arr.Contains(i.column))
                    var contains = Expression.Call(method, new Expression[] { Expression.Constant(obj), member });
                    var lambda = Expression.Lambda<Func<TSource, bool>>(contains, entity);

                    return source.Where(lambda);
                }
                else
                {
                    // Where(i => !arr.Contains(i.column))
                    var notContains = Expression.Not(Expression.Call(method, new Expression[] { Expression.Constant(obj), member }));
                    var lambda = Expression.Lambda<Func<TSource, bool>>(notContains, entity);

                    return source.Where(lambda);
                }
            }
            catch (FormatException)
            {
                throw new JsonApiException(400, $"Could not cast {filter.PropertyValue} to {property.PropertyType.Name}");
            }
        }

        #endregion
    }
}
