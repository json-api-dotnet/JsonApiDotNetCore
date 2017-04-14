using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Http;
using JsonApiDotNetCore.Models;

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
            Fields = new List<string>();
            BuildQuerySet(query);
        }

        public List<FilterQuery> Filters { get; set; }
        public PageQuery PageQuery { get; set; }
        public List<SortQuery> SortParameters { get; set; }
        public List<string> IncludedRelationships { get; set; }
        public List<string> Fields { get; set; }

        private void BuildQuerySet(IQueryCollection query)
        {
            foreach (var pair in query)
            {
                if (pair.Key.StartsWith("filter"))
                {
                    Filters.AddRange(ParseFilterQuery(pair.Key, pair.Value));
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
                    continue;
                }

                if (pair.Key.StartsWith("page"))
                {
                    PageQuery = ParsePageQuery(pair.Key, pair.Value);
                    continue;
                }

                if (pair.Key.StartsWith("fields"))
                {
                    Fields = ParseFieldsQuery(pair.Key, pair.Value);
                    continue;
                }

                throw new JsonApiException("400", $"{pair} is not a valid query.");
            }
        }

        private List<FilterQuery> ParseFilterQuery(string key, string value)
        {
            // expected input = filter[id]=1
            // expected input = filter[id]=eq:1
            var queries = new List<FilterQuery>();

            var propertyName = key.Split('[', ']')[1].ToProperCase();
            
            var values = value.Split(',');
            foreach(var val in values)
            {
                (var operation, var filterValue) = ParseFilterOperation(val);
                queries.Add(new FilterQuery(propertyName, filterValue, operation));
            }

            return queries;
        }

        private (string operation, string value) ParseFilterOperation(string value)
        {
            if(value.Length < 3)
                return (string.Empty, value);
             
            var operation = value.Split(':');

            if(operation.Length == 1)
                return (string.Empty, value);
            
            // remove prefix from value
            var prefix = operation[0];
            value = operation[1];

            return (prefix, value);;
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
            if(value.Contains("."))
                throw new JsonApiException("400", "Deeply nested relationships are not supported");

            return value
                .Split(',')
                .Select(s => s.ToProperCase())
                .ToList();
        }

        private List<string> ParseFieldsQuery(string key, string value)
        {
            // expected: fields[TYPE]=prop1,prop2
            var typeName = key.Split('[', ']')[1];

            var includedFields = new List<string> { "Id" };

            if(typeName != _jsonApiContext.RequestEntity.EntityName) 
                return includedFields;

            var fields = value.Split(',');
            foreach(var field in fields)
            {
                var internalAttrName = _jsonApiContext.RequestEntity
                    .Attributes
                    .SingleOrDefault(attr => attr.PublicAttributeName == field)
                    .InternalAttributeName;
                includedFields.Add(internalAttrName);
            }

            return includedFields;
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