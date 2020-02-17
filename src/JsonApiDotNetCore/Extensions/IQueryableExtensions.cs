using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Query;
using JsonApiDotNetCore.Models;

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
                        .First(m => m.Name == nameof(Enumerable.Contains) && m.GetParameters().Length == 2);
                }
                return _containsMethod;
            }
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

        public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
        {
            foreach (T item in enumeration)
            {
                action(item);
            }
        }

        public static IQueryable<TSource> Filter<TSource>(this IQueryable<TSource> source, FilterQueryContext filterQuery)
        {
            if (filterQuery == null)
                return source;

            if (filterQuery.Operation == FilterOperation.@in || filterQuery.Operation == FilterOperation.nin)
                return CallGenericWhereContainsMethod(source, filterQuery);

            return CallGenericWhereMethod(source, filterQuery);
        }

        public static IQueryable<TSource> Select<TSource>(this IQueryable<TSource> source, IEnumerable<AttrAttribute> columns)
            => CallGenericSelectMethod(source, columns.Select(attr => attr.PropertyInfo.Name).ToList());

        public static IOrderedQueryable<TSource> Sort<TSource>(this IQueryable<TSource> source, SortQueryContext sortQuery)
        {
            return sortQuery.Direction == SortDirection.Descending
                ? source.OrderByDescending(sortQuery.GetPropertyPath())
                : source.OrderBy(sortQuery.GetPropertyPath());
        }

        public static IOrderedQueryable<TSource> Sort<TSource>(this IOrderedQueryable<TSource> source, SortQueryContext sortQuery)
        {

            return sortQuery.Direction == SortDirection.Descending
                ? source.ThenByDescending(sortQuery.GetPropertyPath())
                : source.ThenBy(sortQuery.GetPropertyPath());
        }

        public static IOrderedQueryable<TSource> OrderBy<TSource>(this IQueryable<TSource> source, string propertyName)
            => CallGenericOrderMethod(source, propertyName, "OrderBy");

        public static IOrderedQueryable<TSource> OrderByDescending<TSource>(this IQueryable<TSource> source, string propertyName)
            => CallGenericOrderMethod(source, propertyName, "OrderByDescending");

        public static IOrderedQueryable<TSource> ThenBy<TSource>(this IOrderedQueryable<TSource> source, string propertyName)
            => CallGenericOrderMethod(source, propertyName, "ThenBy");

        public static IOrderedQueryable<TSource> ThenByDescending<TSource>(this IOrderedQueryable<TSource> source, string propertyName)
            => CallGenericOrderMethod(source, propertyName, "ThenByDescending");

        private static IOrderedQueryable<TSource> CallGenericOrderMethod<TSource>(IQueryable<TSource> source, string propertyName, string method)
        {
            // {x}
            var parameter = Expression.Parameter(typeof(TSource), "x");
            MemberExpression member;

            var values = propertyName.Split('.');
            if (values.Length > 1)
            {
                var relation = Expression.PropertyOrField(parameter, values[0]);
                // {x.relationship.propertyName}
                member = Expression.Property(relation, values[1]);
            }
            else
            {
                // {x.propertyName}
                member = Expression.Property(parameter, values[0]);
            }
            // {x=>x.propertyName} or {x=>x.relationship.propertyName}
            var lambda = Expression.Lambda(member, parameter);

            // REFLECTION: source.OrderBy(x => x.Property)
            var orderByMethod = typeof(Queryable).GetMethods().First(x => x.Name == method && x.GetParameters().Length == 2);
            var orderByGeneric = orderByMethod.MakeGenericMethod(typeof(TSource), member.Type);
            var result = orderByGeneric.Invoke(null, new object[] { source, lambda });

            return (IOrderedQueryable<TSource>)result;
        }

        private static Expression GetFilterExpressionLambda(Expression left, Expression right, FilterOperation operation)
        {
            Expression body;
            switch (operation)
            {
                case FilterOperation.eq:
                    // {model.Id == 1}
                    body = Expression.Equal(left, right);
                    break;
                case FilterOperation.lt:
                    // {model.Id < 1}
                    body = Expression.LessThan(left, right);
                    break;
                case FilterOperation.gt:
                    // {model.Id > 1}
                    body = Expression.GreaterThan(left, right);
                    break;
                case FilterOperation.le:
                    // {model.Id <= 1}
                    body = Expression.LessThanOrEqual(left, right);
                    break;
                case FilterOperation.ge:
                    // {model.Id >= 1}
                    body = Expression.GreaterThanOrEqual(left, right);
                    break;
                case FilterOperation.like:
                    body = Expression.Call(left, "Contains", null, right);
                    break;
                // {model.Id != 1}
                case FilterOperation.ne:
                    body = Expression.NotEqual(left, right);
                    break;
                case FilterOperation.isnotnull:
                    // {model.Id != null}
                    if (left.Type.IsValueType &&
                        !(left.Type.IsGenericType && left.Type.GetGenericTypeDefinition() == typeof(Nullable<>)))
                    {
                        var nullableType = typeof(Nullable<>).MakeGenericType(left.Type);
                        body = Expression.NotEqual(Expression.Convert(left, nullableType), right);
                    }
                    else
                    {
                        body = Expression.NotEqual(left, right);
                    }
                    break;
                case FilterOperation.isnull:
                    // {model.Id == null}
                    if (left.Type.IsValueType &&
                        !(left.Type.IsGenericType && left.Type.GetGenericTypeDefinition() == typeof(Nullable<>)))
                    {
                        var nullableType = typeof(Nullable<>).MakeGenericType(left.Type);
                        body = Expression.Equal(Expression.Convert(left, nullableType), right);
                    }
                    else
                    {
                        body = Expression.Equal(left, right);
                    }
                    break;
                default:
                    throw new JsonApiException(500, $"Unknown filter operation {operation}");
            }

            return body;
        }

        private static IQueryable<TSource> CallGenericWhereContainsMethod<TSource>(IQueryable<TSource> source, FilterQueryContext filter)
        {
            var concreteType = typeof(TSource);
            var property = concreteType.GetProperty(filter.Attribute.PropertyInfo.Name);

            try
            {
                var propertyValues = filter.Value.Split(QueryConstants.COMMA);
                ParameterExpression entity = Expression.Parameter(concreteType, "entity");
                MemberExpression member;
                if (filter.IsAttributeOfRelationship)
                {
                    var relation = Expression.PropertyOrField(entity, filter.Relationship.InternalRelationshipName);
                    member = Expression.Property(relation, filter.Attribute.PropertyInfo.Name);
                }
                else
                    member = Expression.Property(entity, filter.Attribute.PropertyInfo.Name);

                var method = ContainsMethod.MakeGenericMethod(member.Type);
                var obj = TypeHelper.ConvertListType(propertyValues, member.Type);

                if (filter.Operation == FilterOperation.@in)
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
                throw new JsonApiException(400, $"Could not cast {filter.Value} to {property.PropertyType.Name}");
            }
        }

        /// <summary>
        /// This calls a generic where method.. more explaining to follow
        /// 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        private static IQueryable<TSource> CallGenericWhereMethod<TSource>(IQueryable<TSource> source, FilterQueryContext filter)
        {
            var op = filter.Operation;
            var concreteType = typeof(TSource);
            PropertyInfo relationProperty;
            PropertyInfo property;
            MemberExpression left;
            ConstantExpression right;

            // {model}
            var parameter = Expression.Parameter(concreteType, "model");
            // Is relationship attribute
            if (filter.IsAttributeOfRelationship)
            {
                relationProperty = concreteType.GetProperty(filter.Relationship.InternalRelationshipName);
                if (relationProperty == null)
                    throw new ArgumentException($"'{filter.Relationship.InternalRelationshipName}' is not a valid relationship of '{concreteType}'");

                var relatedType = filter.Relationship.RightType;
                property = relatedType.GetProperty(filter.Attribute.PropertyInfo.Name);
                if (property == null)
                    throw new ArgumentException($"'{filter.Attribute.PropertyInfo.Name}' is not a valid attribute of '{filter.Relationship.InternalRelationshipName}'");

                var leftRelationship = Expression.PropertyOrField(parameter, filter.Relationship.InternalRelationshipName);
                // {model.Relationship}
                left = Expression.PropertyOrField(leftRelationship, property.Name);
            }
            // Is standalone attribute
            else
            {
                property = concreteType.GetProperty(filter.Attribute.PropertyInfo.Name);
                if (property == null)
                    throw new ArgumentException($"'{filter.Attribute.PropertyInfo.Name}' is not a valid property of '{concreteType}'");

                // {model.Id}
                left = Expression.PropertyOrField(parameter, property.Name);
            }

            try
            {
                if (op == FilterOperation.isnotnull || op == FilterOperation.isnull)
                    right = Expression.Constant(null);
                else
                {
                    // convert the incoming value to the target value type
                    // "1" -> 1
                    var convertedValue = TypeHelper.ConvertType(filter.Value, property.PropertyType);
                    // {1}
                    right = Expression.Constant(convertedValue, property.PropertyType);
                }

                var body = GetFilterExpressionLambda(left, right, filter.Operation);
                var lambda = Expression.Lambda<Func<TSource, bool>>(body, parameter);

                return source.Where(lambda);
            }
            catch (FormatException)
            {
                throw new JsonApiException(400, $"Could not cast {filter.Value} to {property.PropertyType.Name}");
            }
        }

        private static IQueryable<TSource> CallGenericSelectMethod<TSource>(IQueryable<TSource> source, List<string> columns)
        {
            List<MemberAssignment> sourceBindings;
            var sourceType = typeof(TSource);
            var parameter = Expression.Parameter(source.ElementType, "x");
            var sourceProperties = new List<string>();

            // Store all property names to it's own related property (name as key)
            var nestedTypesAndProperties = new Dictionary<string, List<string>>();
            foreach (var column in columns)
            {
                var props = column.Split('.');
                if (props.Length > 1) // Nested property
                {
                    if (nestedTypesAndProperties.TryGetValue(props[0], out var properties) == false)
                        nestedTypesAndProperties.Add(props[0], new List<string> { nameof(Identifiable.Id), props[1] });
                    else
                        properties.Add(props[1]);
                }
                else
                    sourceProperties.Add(props[0]);
            }

            // Bind attributes on TSource
            sourceBindings = sourceProperties.Select(prop => Expression.Bind(sourceType.GetProperty(prop), Expression.PropertyOrField(parameter, prop))).ToList();

            // Bind attributes on nested types
            var nestedBindings = new List<MemberAssignment>();
            Expression bindExpression;
            foreach (var item in nestedTypesAndProperties)
            {
                var nestedProperty = sourceType.GetProperty(item.Key);
                var nestedPropertyType = nestedProperty.PropertyType;
                // [HasMany] attribute
                if (nestedPropertyType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(nestedPropertyType))
                {
                    // Concrete type of Collection
                    var singleType = nestedPropertyType.GetGenericArguments().Single();
                    // {y}
                    var nestedParameter = Expression.Parameter(singleType, "y");
                    nestedBindings = item.Value.Select(prop => Expression.Bind(
                        singleType.GetProperty(prop), Expression.PropertyOrField(nestedParameter, prop))).ToList();

                    // { new Item() }
                    var newNestedExp = Expression.New(singleType);
                    var initNestedExp = Expression.MemberInit(newNestedExp, nestedBindings);
                    // { y => new Item() {Id = y.Id, Name = y.Name}}
                    var body = Expression.Lambda(initNestedExp, nestedParameter);
                    // { x.Items }
                    Expression propertyExpression = Expression.Property(parameter, nestedProperty.Name);
                    // { x.Items.Select(y => new Item() {Id = y.Id, Name = y.Name}) }
                    Expression selectMethod = Expression.Call(
                        typeof(Enumerable),
                        "Select",
                        new[] { singleType, singleType },
                        propertyExpression, body);

                    // { x.Items.Select(y => new Item() {Id = y.Id, Name = y.Name}).ToList() }
                    bindExpression = Expression.Call(
                         typeof(Enumerable),
                         "ToList",
                         new[] { singleType },
                         selectMethod);
                }
                // [HasOne] attribute
                else
                {
                    // {x.Owner}
                    var srcBody = Expression.PropertyOrField(parameter, item.Key);
                    foreach (var nested in item.Value)
                    {
                        // {x.Owner.Name}
                        var nestedBody = Expression.PropertyOrField(srcBody, nested);
                        var propInfo = nestedPropertyType.GetProperty(nested);
                        nestedBindings.Add(Expression.Bind(propInfo, nestedBody));
                    }
                    // { new Owner() }
                    var newExp = Expression.New(nestedPropertyType);
                    // { new Owner() { Id = x.Owner.Id, Name = x.Owner.Name }}
                    var newInit = Expression.MemberInit(newExp, nestedBindings);

                    // Handle nullable relationships
                    // { Owner = x.Owner == null ? null : new Owner() {...} }
                    bindExpression = Expression.Condition(
                           Expression.Equal(srcBody, Expression.Constant(null)),
                           Expression.Convert(Expression.Constant(null), nestedPropertyType),
                           newInit
                         );
                }

                sourceBindings.Add(Expression.Bind(nestedProperty, bindExpression));
                nestedBindings.Clear();
            }

            var sourceInit = Expression.MemberInit(Expression.New(sourceType), sourceBindings);
            var finalBody = Expression.Lambda(sourceInit, parameter);

            return source.Provider.CreateQuery<TSource>(Expression.Call(
                typeof(Queryable),
                "Select",
                new[] { source.ElementType, typeof(TSource) },
                source.Expression,
                Expression.Quote(finalBody)));
        }
    }
}
