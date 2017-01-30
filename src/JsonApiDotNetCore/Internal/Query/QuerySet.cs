using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCore.Internal.Query
{
    public class QuerySet
    {
        IJsonApiContext _jsonApiContext;

        public QuerySet(
            IJsonApiContext jsonApiContext, 
            IQueryCollection query)
        {
            _jsonApiContext = jsonApiContext;
            PageQuery = new PageQuery();
            Filters = new List<FilterQuery>();
            BuildQuerySet(query);
        }

        public List<FilterQuery> Filters { get; set; }
        public PageQuery PageQuery { get; set; }
        public List<SortQuery> SortParameters { get; set; }
        public List<string> IncludedRelationships { get; set; }

        private void BuildQuerySet(IQueryCollection query)
        {
            foreach (var pair in query)
            {
                if (pair.Key.StartsWith("filter"))
                {
                    Filters.Add(ParseFilterQuery(pair.Key, pair.Value));
                    continue;
                }

                if (pair.Key.StartsWith("sort"))
                {
                    SortParameters = ParseSortParameters(pair.Value);
                    continue;
                }

                if (pair.Key.StartsWith("include"))
                {
                    IncludedRelationships = ParseIncludedRelationships(pair.Value);
                }

                if (pair.Key.StartsWith("page"))
                {
                    PageQuery = ParsePageQuery(pair.Key, pair.Value);
                }
            }
        }

        private FilterQuery ParseFilterQuery(string key, string value)
        {
            // expected input = filter[id]=1
            var propertyName = key.Split('[', ']')[1].ToProperCase();
            var attribute = GetAttribute(propertyName);

            if (attribute == null)
                throw new JsonApiException("400", $"{propertyName} is not a valid property.");
            
            return new FilterQuery(attribute, value);
        }

        private PageQuery ParsePageQuery(string key, string value)
        {
            // expected input = page[size]=10
            //                  page[number]=1
            PageQuery = PageQuery ?? new PageQuery();

            var propertyName = key.Split('[', ']')[1];
            
            if (propertyName == "size")
                PageQuery.PageSize = Convert.ToInt32(value);
            else if (propertyName == "number")
                PageQuery.PageOffset = Convert.ToInt32(value);

            return PageQuery;
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

                var attribute = GetAttribute(p.ToProperCase());

                sortParameters.Add(new SortQuery(direction, attribute));
            });

            return sortParameters;
        }

        private List<string> ParseIncludedRelationships(string value)
        {
            return value
                .Split(',')
                .Select(s => s.ToProperCase())
                .ToList();
        }

        private AttrAttribute GetAttribute(string propertyName)
        {
            return _jsonApiContext.RequestEntity.Attributes
                .FirstOrDefault(attr =>
                    attr.InternalAttributeName.ToLower() == propertyName.ToLower()
            );
        }
    }
}