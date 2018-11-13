using System;
using System.Collections;
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

        [Obsolete("Use overload Sort<T>(IJsonApiContext, List<SortQuery>) instead.", error: true)]
        public static IQueryable<TSource> Sort<TSource>(this IQueryable<TSource> source, List<SortQuery> sortQueries) => null;

        [Obsolete("Use overload Sort<T>(IJsonApiContext, SortQuery) instead.", error: true)]
        public static IOrderedQueryable<TSource> Sort<TSource>(this IQueryable<TSource> source, SortQuery sortQuery) => null;

        [Obsolete("Use overload Sort<T>(IJsonApiContext, SortQuery) instead.", error: true)]
        public static IOrderedQueryable<TSource> Sort<TSource>(this IOrderedQueryable<TSource> source, SortQuery sortQuery) => null;

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
            BaseAttrQuery attr;
            if (sortQuery.IsAttributeOfRelationship)
                attr = new RelatedAttrSortQuery(jsonApiContext, sortQuery);
            else
                attr = new AttrSortQuery(jsonApiContext, sortQuery);

            return sortQuery.Direction == SortDirection.Descending
                ? source.OrderByDescending(attr.GetPropertyPath())
                : source.OrderBy(attr.GetPropertyPath());
        }

        public static IOrderedQueryable<TSource> Sort<TSource>(this IOrderedQueryable<TSource> source, IJsonApiContext jsonApiContext, SortQuery sortQuery)
        {
            BaseAttrQuery attr;
            if (sortQuery.IsAttributeOfRelationship)
                attr = new RelatedAttrSortQuery(jsonApiContext, sortQuery);
            else
                attr = new AttrSortQuery(jsonApiContext, sortQuery);

            return sortQuery.Direction == SortDirection.Descending
                ? source.ThenByDescending(attr.GetPropertyPath())
                : source.ThenBy(attr.GetPropertyPath());
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

        public static IQueryable<TSource> Filter<TSource>(this IQueryable<TSource> source, IJsonApiContext jsonApiContext, FilterQuery filterQuery)
        {
            if (filterQuery == null)
                return source;

            // Relationship.Attribute
            if (filterQuery.IsAttributeOfRelationship)
                return source.Filter(new RelatedAttrFilterQuery(jsonApiContext, filterQuery));

            return source.Filter(new AttrFilterQuery(jsonApiContext, filterQuery));
        }

        public static IQueryable<TSource> Filter<TSource>(this IQueryable<TSource> source, BaseFilterQuery filterQuery)
        {
            if (filterQuery == null)
                return source;

            if (filterQuery.FilterOperation == FilterOperations.@in || filterQuery.FilterOperation == FilterOperations.nin)
                return CallGenericWhereContainsMethod(source, filterQuery);
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

        private static IQueryable<TSource> CallGenericWhereContainsMethod<TSource>(IQueryable<TSource> source, BaseFilterQuery filter)
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
                    var relation = Expression.PropertyOrField(entity, filter.Relationship.InternalRelationshipName);
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

        private static IQueryable<TSource> CallGenericWhereMethod<TSource>(IQueryable<TSource> source, BaseFilterQuery filter)
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
                relationProperty = concreteType.GetProperty(filter.Relationship.InternalRelationshipName);
                if (relationProperty == null)
                    throw new ArgumentException($"'{filter.Relationship.InternalRelationshipName}' is not a valid relationship of '{concreteType}'");

                var relatedType = filter.Relationship.Type;
                property = relatedType.GetProperty(filter.Attribute.InternalAttributeName);
                if (property == null)
                    throw new ArgumentException($"'{filter.Attribute.InternalAttributeName}' is not a valid attribute of '{filter.Relationship.InternalRelationshipName}'");

                var leftRelationship = Expression.PropertyOrField(parameter, filter.Relationship.InternalRelationshipName);
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

        public static IQueryable<TSource> Select<TSource>(this IQueryable<TSource> source, List<string> columns)
            => CallGenericSelectMethod(source, columns);

        private static IQueryable<TSource> CallGenericSelectMethod<TSource>(IQueryable<TSource> source, List<string> columns)
        {
            var sourceBindings = new List<MemberAssignment>();
            var sourceType = typeof(TSource);
            var parameter = Expression.Parameter(source.ElementType, "x");
            var sourceProperties = new List<string>() { };

            // Store all property names to it's own related property (name as key)
            var nestedTypesAndProperties = new Dictionary<string, List<string>>();
            foreach (var column in columns)
            {
                var props = column.Split('.');
                if (props.Length > 1) // Nested property
                {
                    if (nestedTypesAndProperties.TryGetValue(props[0], out var properties) == false)
                        nestedTypesAndProperties.Add(props[0], new List<string>() { nameof(Identifiable.Id), props[1] });
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
                        new Type[] { singleType, singleType },
                        propertyExpression, body);

                    // { x.Items.Select(y => new Item() {Id = y.Id, Name = y.Name}).ToList() }
                    bindExpression = Expression.Call(
                         typeof(Enumerable),
                         "ToList",
                         new Type[] { singleType },
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
