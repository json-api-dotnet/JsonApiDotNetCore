using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Query;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.Extensions
{
    // ReSharper disable once InconsistentNaming
    public static class IQueryableExtensions
    {
        public static IQueryable<TSource> Sort<TSource>(this IQueryable<TSource> source, List<SortQuery> sortQueries)
        {
            if (sortQueries == null || sortQueries.Count == 0)
                return source;

            var orderedEntities = source.Sort(sortQueries[0]);

            if (sortQueries.Count <= 1) return orderedEntities;

            for (var i = 1; i < sortQueries.Count; i++)
                orderedEntities = orderedEntities.Sort(sortQueries[i]);

            return orderedEntities;
        }

        public static IOrderedQueryable<TSource> Sort<TSource>(this IQueryable<TSource> source, SortQuery sortQuery)
        {
            return sortQuery.Direction == SortDirection.Descending 
                ? source.OrderByDescending(sortQuery.SortedAttribute.InternalAttributeName) 
                : source.OrderBy(sortQuery.SortedAttribute.InternalAttributeName);
        }

        public static IOrderedQueryable<TSource> Sort<TSource>(this IOrderedQueryable<TSource> source, SortQuery sortQuery)
        {
            return sortQuery.Direction == SortDirection.Descending 
                ? source.ThenByDescending(sortQuery.SortedAttribute.InternalAttributeName) 
                : source.ThenBy(sortQuery.SortedAttribute.InternalAttributeName);
        }

        public static IOrderedQueryable<TSource> OrderBy<TSource>(this IQueryable<TSource> source, string propertyName)
        {
            return CallGenericOrderMethod(source, propertyName, "OrderBy");
        }

        public static IOrderedQueryable<TSource> OrderByDescending<TSource>(this IQueryable<TSource> source, string propertyName)
        {
            return CallGenericOrderMethod(source, propertyName, "OrderByDescending");
        }

        public static IOrderedQueryable<TSource> ThenBy<TSource>(this IOrderedQueryable<TSource> source, string propertyName)
        {
            return CallGenericOrderMethod(source, propertyName, "ThenBy");
        }

        public static IOrderedQueryable<TSource> ThenByDescending<TSource>(this IOrderedQueryable<TSource> source, string propertyName)
        {
            return CallGenericOrderMethod(source, propertyName, "ThenByDescending");
        }

        private static IOrderedQueryable<TSource> CallGenericOrderMethod<TSource>(IQueryable<TSource> source, string propertyName, string method)
        {
            // {x}
            var parameter = Expression.Parameter(typeof(TSource), "x");
            // {x.propertyName}
            var property = Expression.Property(parameter, propertyName);
            // {x=>x.propertyName}
            var lambda = Expression.Lambda(property, parameter);

            // REFLECTION: source.OrderBy(x => x.Property)
            var orderByMethod = typeof(Queryable).GetMethods().First(x => x.Name == method && x.GetParameters().Length == 2);
            var orderByGeneric = orderByMethod.MakeGenericMethod(typeof(TSource), property.Type);
            var result = orderByGeneric.Invoke(null, new object[] { source, lambda });

            return (IOrderedQueryable<TSource>)result;
        }

        public static IQueryable<TSource> Filter<TSource>(this IQueryable<TSource> source, IJsonApiContext jsonApiContext, FilterQuery filterQuery)
        {
            if (filterQuery == null)
                return source;

            if (filterQuery.IsAttributeOfRelationship)
                return source.Filter(new RelatedAttrFilterQuery(jsonApiContext, filterQuery));

            return source.Filter(new AttrFilterQuery(jsonApiContext, filterQuery));
        }

        public static IQueryable<TSource> Filter<TSource>(this IQueryable<TSource> source, AttrFilterQuery filterQuery)
        {
            if (filterQuery == null)
                return source;

            var concreteType = typeof(TSource);
            var property = concreteType.GetProperty(filterQuery.FilteredAttribute.InternalAttributeName);

            if (property == null)
                throw new ArgumentException($"'{filterQuery.FilteredAttribute.InternalAttributeName}' is not a valid property of '{concreteType}'");

            try
            {
                // convert the incoming value to the target value type
                // "1" -> 1
                var convertedValue = TypeHelper.ConvertType(filterQuery.PropertyValue, property.PropertyType);
                // {model}
                var parameter = Expression.Parameter(concreteType, "model");
                // {model.Id}
                var left = Expression.PropertyOrField(parameter, property.Name);
                // {1}
                var right = Expression.Constant(convertedValue, property.PropertyType);

                var body = GetFilterExpressionLambda(left, right, filterQuery.FilterOperation);

                var lambda = Expression.Lambda<Func<TSource, bool>>(body, parameter);

                return source.Where(lambda);
            }
            catch (FormatException)
            {
                throw new JsonApiException(400, $"Could not cast {filterQuery.PropertyValue} to {property.PropertyType.Name}");
            }
        }

        public static IQueryable<TSource> Filter<TSource>(this IQueryable<TSource> source, RelatedAttrFilterQuery filterQuery)
        {
            if (filterQuery == null)
                return source;

            var concreteType = typeof(TSource);
            var relation = concreteType.GetProperty(filterQuery.FilteredRelationship.InternalRelationshipName);
            if (relation == null)
                throw new ArgumentException($"'{filterQuery.FilteredRelationship.InternalRelationshipName}' is not a valid relationship of '{concreteType}'");

            var relatedType = filterQuery.FilteredRelationship.Type;
            var relatedAttr = relatedType.GetProperty(filterQuery.FilteredAttribute.InternalAttributeName);
            if (relatedAttr == null)
                throw new ArgumentException($"'{filterQuery.FilteredAttribute.InternalAttributeName}' is not a valid attribute of '{filterQuery.FilteredRelationship.InternalRelationshipName}'");

            try
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
            catch (FormatException)
            {
                throw new JsonApiException(400, $"Could not cast {filterQuery.PropertyValue} to {relatedAttr.PropertyType.Name}");
            }
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
                default:
                    throw new JsonApiException(500, $"Unknown filter operation {operation}");
            }

            return body;
        }

        public static IQueryable<TSource> Select<TSource>(this IQueryable<TSource> source, List<string> columns)
        {
            if (columns == null || columns.Any() == false)
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

        public static IQueryable<T> Page<T>(this IQueryable<T> source, int pageSize, int pageNumber)
        {
            if (pageSize > 0)
            {
                if (pageNumber == 0)
                    pageNumber = 1;

                if (pageNumber > 0)
                    return source
                        .Skip((pageNumber - 1) * pageSize)
                        .Take(pageSize);
                else // page from the end of the set
                    return source
                        .Skip((Math.Abs(pageNumber) - 1) * pageSize)
                        .Take(pageSize);
            }

            return source;
        }
    }
}
