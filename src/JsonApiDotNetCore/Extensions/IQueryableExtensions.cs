using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Query;

namespace JsonApiDotNetCore.Extensions
{
    public static class IQueryableExtensions
    {
        public static IOrderedQueryable<TSource> Sort<TSource>(this IQueryable<TSource> source, SortQuery sortQuery)
        {
            if (sortQuery.Direction == SortDirection.Descending)
                return source.OrderByDescending(sortQuery.SortedAttribute.InternalAttributeName);

            return source.OrderBy(sortQuery.SortedAttribute.InternalAttributeName);
        }

        public static IOrderedQueryable<TSource> Sort<TSource>(this IOrderedQueryable<TSource> source, SortQuery sortQuery)
        {
            if (sortQuery.Direction == SortDirection.Descending)
                return source.ThenByDescending(sortQuery.SortedAttribute.InternalAttributeName);

            return source.ThenBy(sortQuery.SortedAttribute.InternalAttributeName);
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

                Expression body;
                switch (filterQuery.FilterOperation)
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
                        // {model.Id <= 1}
                        body = Expression.GreaterThanOrEqual(left, right);
                        break;
                    case FilterOperations.like:
                        // {model.Id <= 1}
                        body = Expression.Call(left, "Contains", null, right);
                        break;
                    default:
                        throw new JsonApiException("500", $"Unknown filter operation {filterQuery.FilterOperation}");
                }

                var lambda = Expression.Lambda<Func<TSource, bool>>(body, parameter);

                return source.Where(lambda);
            }
            catch (FormatException)
            {
                throw new JsonApiException("400", $"Could not cast {filterQuery.PropertyValue} to {property.PropertyType.Name}");
            }
        }
        public static IQueryable<TSource> Select<TSource>(this IQueryable<TSource> source, IEnumerable<string> columns)
        {
            if(columns == null || columns.Count() == 0)
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
                Expression.Call(typeof(Queryable), "Select", new Type[] { sourceType, resultType },
                source.Expression, Expression.Quote(selector)));
        }
    }
}
