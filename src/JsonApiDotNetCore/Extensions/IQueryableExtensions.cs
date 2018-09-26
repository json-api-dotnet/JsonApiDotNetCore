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

        public static IOrderedQueryable<TSource> OrderBy<TSource>(this IQueryable<TSource> source, AttrQuery attrQuery)
        {
            return CallGenericOrderMethod(source, attrQuery.Attribute, null, "OrderBy");
        }
        public static IOrderedQueryable<TSource> OrderBy<TSource>(this IQueryable<TSource> source, RelatedAttrQuery relatedAttrQuery)
        {
            return CallGenericOrderMethod(source, relatedAttrQuery.Attribute, relatedAttrQuery.RelationshipAttribute, "OrderBy");
        }

        public static IOrderedQueryable<TSource> OrderByDescending<TSource>(this IQueryable<TSource> source, AttrQuery attrQuery)
        {
            return CallGenericOrderMethod(source, attrQuery.Attribute, null, "OrderByDescending");
        }
        public static IOrderedQueryable<TSource> OrderByDescending<TSource>(this IQueryable<TSource> source, RelatedAttrQuery relatedAttrQuery)
        {
            return CallGenericOrderMethod(source, relatedAttrQuery.Attribute, relatedAttrQuery.RelationshipAttribute, "OrderByDescending");
        }

        public static IOrderedQueryable<TSource> ThenBy<TSource>(this IOrderedQueryable<TSource> source, AttrQuery attrQuery)
        {
            return CallGenericOrderMethod(source, attrQuery.Attribute, null, "ThenBy");
        }
        public static IOrderedQueryable<TSource> ThenBy<TSource>(this IOrderedQueryable<TSource> source, RelatedAttrQuery relatedAttrQuery)
        {
            return CallGenericOrderMethod(source, relatedAttrQuery.Attribute, relatedAttrQuery.RelationshipAttribute, "ThenBy");
        }

        public static IOrderedQueryable<TSource> ThenByDescending<TSource>(this IOrderedQueryable<TSource> source, AttrQuery attrQuery)
        {
            return CallGenericOrderMethod(source, attrQuery.Attribute, null, "ThenByDescending");
        }
        public static IOrderedQueryable<TSource> ThenByDescending<TSource>(this IOrderedQueryable<TSource> source, RelatedAttrQuery relatedAttrQuery)
        {
            return CallGenericOrderMethod(source, relatedAttrQuery.Attribute, relatedAttrQuery.RelationshipAttribute, "ThenByDescending");
        }

        private static IOrderedQueryable<TSource> CallGenericOrderMethod<TSource>(IQueryable<TSource> source, AttrAttribute attr, RelationshipAttribute relationAttr, string method)
        {
            // {x}
            var parameter = Expression.Parameter(typeof(TSource), "x");
            
            //var property = Expression.Property(parameter, attr.InternalAttributeName);

            MemberExpression member;
            // {x.relationship.propertyName}
            if (relationAttr != null)
            {
                var relation = Expression.PropertyOrField(parameter, relationAttr.InternalRelationshipName);
                member = Expression.Property(relation, attr.InternalAttributeName);
            }
            // {x.propertyName}
            else
                member = Expression.Property(parameter, attr.InternalAttributeName);

            // {x=>x.propertyName} or {x=>x.relationship.propertyName}
            var lambda = Expression.Lambda(member, parameter);

            // REFLECTION: source.OrderBy(x => x.Property)
            var orderByMethod = typeof(Queryable).GetMethods().First(x => x.Name == method && x.GetParameters().Length == 2);
            var orderByGeneric = orderByMethod.MakeGenericMethod(typeof(TSource), member.Type);
            var result = orderByGeneric.Invoke(null, new object[] { source, lambda });

            return (IOrderedQueryable<TSource>)result;
        }

        public static IQueryable<TSource> Filter<TSource>(this IQueryable<TSource> source, IJsonApiContext jsonApiContext, FilterQuery filterQuery)
        {
            if (filterQuery == null)
                return source;

            if (filterQuery.IsAttributeOfRelationship)
                return source.Filter(new RelatedAttrQuery(jsonApiContext, filterQuery));

            return source.Filter(new AttrQuery(jsonApiContext, filterQuery));
        }

        public static IQueryable<TSource> Filter<TSource>(this IQueryable<TSource> source, AttrQuery filterQuery)
        {
            if (filterQuery == null)
                return source;

            var concreteType = typeof(TSource);
            var property = concreteType.GetProperty(filterQuery.Attribute.InternalAttributeName);
            var op = filterQuery.FilterOperation;

            if (property == null)
                throw new ArgumentException($"'{filterQuery.Attribute.InternalAttributeName}' is not a valid property of '{concreteType}'");

            try
            {
                if (op == FilterOperations.@in || op == FilterOperations.nin)
                {
                    string[] propertyValues = filterQuery.PropertyValue.Split(',');
                    var lambdaIn = ArrayContainsPredicate<TSource>(propertyValues, property.Name, op);

                    return source.Where(lambdaIn);
                }
                else if (op == FilterOperations.isnotnull || op == FilterOperations.isnull) {
                    // {model}
                    var parameter = Expression.Parameter(concreteType, "model");
                    // {model.Id}
                    var left = Expression.PropertyOrField(parameter, property.Name);
                    var right = Expression.Constant(null);

                    var body = GetFilterExpressionLambda(left, right, op);
                    var lambda = Expression.Lambda<Func<TSource, bool>>(body, parameter);

                    return source.Where(lambda);
                }
                else
                {   // convert the incoming value to the target value type
                    // "1" -> 1
                    var convertedValue = TypeHelper.ConvertType(filterQuery.PropertyValue, property.PropertyType);
                    // {model}
                    var parameter = Expression.Parameter(concreteType, "model");
                    // {model.Id}
                    var left = Expression.PropertyOrField(parameter, property.Name);
                    // {1}
                    var right = Expression.Constant(convertedValue, property.PropertyType);

                    var body = GetFilterExpressionLambda(left, right, op);

                    var lambda = Expression.Lambda<Func<TSource, bool>>(body, parameter);

                    return source.Where(lambda);
                }
            }
            catch (FormatException)
            {
                throw new JsonApiException(400, $"Could not cast {filterQuery.PropertyValue} to {property.PropertyType.Name}");
            }
        }

        public static IQueryable<TSource> Filter<TSource>(this IQueryable<TSource> source, RelatedAttrQuery filterQuery)
        {
            if (filterQuery == null)
                return source;

            var concreteType = typeof(TSource);
            var relation = concreteType.GetProperty(filterQuery.RelationshipAttribute.InternalRelationshipName);
            if (relation == null)
                throw new ArgumentException($"'{filterQuery.RelationshipAttribute.InternalRelationshipName}' is not a valid relationship of '{concreteType}'");

            var relatedType = filterQuery.RelationshipAttribute.Type;
            var relatedAttr = relatedType.GetProperty(filterQuery.Attribute.InternalAttributeName);
            if (relatedAttr == null)
                throw new ArgumentException($"'{filterQuery.Attribute.InternalAttributeName}' is not a valid attribute of '{filterQuery.RelationshipAttribute.InternalRelationshipName}'");

            try
            {
                if (filterQuery.FilterOperation == FilterOperations.@in || filterQuery.FilterOperation == FilterOperations.nin)
                {
                    string[] propertyValues = filterQuery.PropertyValue.Split(',');
                    var lambdaIn = ArrayContainsPredicate<TSource>(propertyValues, relatedAttr.Name, filterQuery.FilterOperation, relation.Name);

                    return source.Where(lambdaIn);
                }
                else
                {
                    // convert the incoming value to the target value type
                    // "1" -> 1
                    var convertedValue = TypeHelper.ConvertType(filterQuery.PropertyValue, relatedAttr.PropertyType);
                    // {model}
                    var parameter = Expression.Parameter(concreteType, "model");

                    // {model.Relationship}
                    var leftRelationship = Expression.PropertyOrField(parameter, relation.Name);

                    // {model.Relationship.Attr}
                    var left = Expression.PropertyOrField(leftRelationship, relatedAttr.Name);

                    // {1}
                    var right = Expression.Constant(convertedValue, relatedAttr.PropertyType);

                    var body = GetFilterExpressionLambda(left, right, filterQuery.FilterOperation);

                    var lambda = Expression.Lambda<Func<TSource, bool>>(body, parameter);

                    return source.Where(lambda);
                }
            }
            catch (FormatException)
            {
                throw new JsonApiException(400, $"Could not cast {filterQuery.PropertyValue} to {relatedAttr.PropertyType.Name}");
            }
        }

        private static bool IsNullable(Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);


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

        private static Expression<Func<TSource, bool>> ArrayContainsPredicate<TSource>(string[] propertyValues, string fieldname, FilterOperations op, string relationName = null)
        {
            ParameterExpression entity = Expression.Parameter(typeof(TSource), "entity");
            MemberExpression member;
            if (!string.IsNullOrEmpty(relationName))
            {
                var relation = Expression.PropertyOrField(entity, relationName);
                member = Expression.Property(relation, fieldname);
            }
            else
                member = Expression.Property(entity, fieldname);

            var method = ContainsMethod.MakeGenericMethod(member.Type);
            var obj = TypeHelper.ConvertListType(propertyValues, member.Type);

            if (op == FilterOperations.@in)
            {
                // Where(i => arr.Contains(i.column))
                var contains = Expression.Call(method, new Expression[] { Expression.Constant(obj), member });
                return Expression.Lambda<Func<TSource, bool>>(contains, entity);
            }
            else
            {
                // Where(i => !arr.Contains(i.column))
                var notContains = Expression.Not(Expression.Call(method, new Expression[] { Expression.Constant(obj), member }));
                return Expression.Lambda<Func<TSource, bool>>(notContains, entity);
            }
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
    }
}
