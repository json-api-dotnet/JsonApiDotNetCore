using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCore.Extensions;

namespace JsonApiDotNetCore.Internal.Query
{
    public class QuerySet<T>
    {
        IJsonApiContext _jsonApiContext;

        public QuerySet(IJsonApiContext jsonApiContext)
        {
            _jsonApiContext = jsonApiContext;
            BuildQuerySet();
        }

        public FilterQuery Filter { get; set; }
        public List<SortQuery> SortParameters { get; set; }

        private void BuildQuerySet()
        {
            foreach (var pair in _jsonApiContext.Query)
            {
                if (pair.Key.StartsWith("filter"))
                {
                    Filter = ParseFilterQuery(pair.Key, pair.Value);
                    continue;
                }

                if (pair.Key.StartsWith("sort"))
                {
                    SortParameters = ParseSortParameters(pair.Value);
                }
            }
        }

        private FilterQuery ParseFilterQuery(string key, string value)
        {
            // expected input = filter[id]=1
            var propertyName = key.Split('[', ']')[1];
            var attribute = GetAttribute(propertyName);

            if(attribute == null)
                return null;

            return new FilterQuery(attribute, value);
        }

        // sort=id,name
        // sort=-id
        private List<SortQuery> ParseSortParameters(string value)
        {
            var sortParameters = new List<SortQuery>();
            value.Split(',').ToList().ForEach(p =>
            {
                var direction = SortDirection.Ascending;
                if (p[0] == '-')
                {
                    direction = SortDirection.Descending;
                    p = p.Substring(1);
                }

                var attribute = GetAttribute(p);

                sortParameters.Add(new SortQuery(direction, attribute));
            });

            return sortParameters;
        }

        private AttrAttribute GetAttribute(string propertyName)
        {
            return _jsonApiContext.RequestEntity.Attributes
                .FirstOrDefault(attr => 
                    attr.InternalAttributeName.ToLower() == propertyName.ToLower()
            );
        }

        public IQueryable<T> ApplyQuery(IQueryable<T> entities)
        {
            entities = ApplyFilter(entities);
            entities = ApplySort(entities);
            return entities;
        }

        private IQueryable<T> ApplyFilter(IQueryable<T> entities)
        {
            if(Filter == null)
                return entities;

            var expression = GetEqualityExpressionForProperty(entities, 
                Filter.FilteredAttribute.InternalAttributeName, Filter.PropertyValue);

            return entities.Where(expression);
        }

        private IQueryable<T> ApplySort(IQueryable<T> entities)
        {
            if(SortParameters == null || SortParameters.Count == 0)
                return entities;
            
            SortParameters.ForEach(sortParam => {
                if(sortParam.Direction == SortDirection.Ascending)
                    entities = entities.OrderBy(sortParam.SortedAttribute.InternalAttributeName);
                else
                    entities = entities.OrderByDescending(sortParam.SortedAttribute.InternalAttributeName);
            });

            return entities;
        }

        private Expression<Func<T, bool>> GetEqualityExpressionForProperty(IQueryable<T> query, string param, object value)
        {
            var currentType = query.ElementType;
            var property = currentType.GetProperty(param);

            if (property == null)
                throw new ArgumentException($"'{param}' is not a valid property of '{currentType}'");

            // convert the incoming value to the target value type
            // "1" -> 1
            var convertedValue = Convert.ChangeType(value, property.PropertyType);
            // {model}
            var prm = Expression.Parameter(currentType, "model");
            // {model.Id}
            var left = Expression.PropertyOrField(prm, property.Name);
            // {1}
            var right = Expression.Constant(convertedValue, property.PropertyType);
            // {model.Id == 1}
            var body = Expression.Equal(left, right);

            return Expression.Lambda<Func<T, bool>>(body, prm);
        }
    }
}