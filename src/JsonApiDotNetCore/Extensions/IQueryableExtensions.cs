
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JsonApiDotNetCore.Internal.Query;

namespace JsonApiDotNetCore.Extensions
{
    public static class IQueryableExtensions
    {
        public static IOrderedQueryable<TSource> Sort<TSource>(this IQueryable<TSource> source, SortQuery sortQuery)
        {
            if(sortQuery.Direction == SortDirection.Descending)
                return source.OrderByDescending(sortQuery.SortedAttribute.InternalAttributeName);
                
            return source.OrderBy(sortQuery.SortedAttribute.InternalAttributeName);
        }

        public static IOrderedQueryable<TSource> Sort<TSource>(this IOrderedQueryable<TSource> source, SortQuery sortQuery)
        {
            if(sortQuery.Direction == SortDirection.Descending)
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

        public static IQueryable<TSource> Filter<TSource>(this IQueryable<TSource> source, FilterQuery filterQuery)
        {
            if(filterQuery == null)
                return source;

            var concreteType = typeof(TSource);
            var property = concreteType.GetProperty(filterQuery.FilteredAttribute.InternalAttributeName);

            if (property == null)
                throw new ArgumentException($"'{filterQuery.FilteredAttribute.InternalAttributeName}' is not a valid property of '{concreteType}'");

            // convert the incoming value to the target value type
            // "1" -> 1
            var convertedValue = Convert.ChangeType(filterQuery.PropertyValue, property.PropertyType);
            // {model}
            var parameter = Expression.Parameter(concreteType, "model");
            // {model.Id}
            var left = Expression.PropertyOrField(parameter, property.Name);
            // {1}
            var right = Expression.Constant(convertedValue, property.PropertyType);
            // {model.Id == 1}
            var body = Expression.Equal(left, right);

            var lambda = Expression.Lambda<Func<TSource, bool>>(body, parameter);

            return source.Where(lambda);
        }
    }
}
