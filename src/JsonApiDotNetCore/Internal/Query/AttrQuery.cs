using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using System;
using System.Linq;

namespace JsonApiDotNetCore.Internal.Query
{
    public class AttrQuery
    {
        private readonly IJsonApiContext _jsonApiContext;
        public AttrAttribute Attribute { get; }

        // Filter properties
        public string PropertyValue { get; }
        public FilterOperations FilterOperation { get; }
        // Sort properties
        public SortDirection Direction { get; set; }

        /// <summary>
        /// Build AttrQuery base on FilterQuery values.
        /// </summary>
        /// <param name="jsonApiContext"></param>
        /// <param name="query"></param>
        public AttrQuery(IJsonApiContext jsonApiContext, FilterQuery query)
        {
            _jsonApiContext = jsonApiContext;
            Attribute = GetAttribute(query.Attribute);

            if (Attribute.IsFilterable == false)
                throw new JsonApiException(400, $"Filter is not allowed for attribute '{Attribute.PublicAttributeName}'.");

            PropertyValue = query.Value;
            FilterOperation = query.OperationType;
        }

        /// <summary>
        /// Build AttrQuery base on SortQuery values.
        /// </summary>
        /// <param name="jsonApiContext"></param>
        /// <param name="query"></param>
        public AttrQuery(IJsonApiContext jsonApiContext, SortQuery sortQuery)
        {
            _jsonApiContext = jsonApiContext;
            Attribute = GetAttribute(sortQuery.Attribute);

            if (Attribute.IsSortable == false)
                throw new JsonApiException(400, $"Sort is not allowed for attribute '{Attribute.PublicAttributeName}'.");

            Direction = sortQuery.Direction;
        }

        private AttrAttribute GetAttribute(string attribute)
        {
            try
            {
                return _jsonApiContext
                    .RequestEntity
                    .Attributes
                    .Single(attr => attr.Is(attribute));
            }
            catch (InvalidOperationException e)
            {
                throw new JsonApiException(400, $"Attribute '{attribute}' does not exist on resource '{_jsonApiContext.RequestEntity.EntityName}'", e);
            }
        }

    }
}
