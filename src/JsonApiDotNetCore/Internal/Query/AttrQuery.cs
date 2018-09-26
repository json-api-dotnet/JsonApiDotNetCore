using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using System;
using System.Linq;

namespace JsonApiDotNetCore.Internal.Query
{
    public class AttrQuery : BaseAttrQuery
    {
        private readonly IJsonApiContext _jsonApiContext;
        private readonly bool _isAttributeOfRelationship = false;

        /// <summary>
        /// Build AttrQuery based on FilterQuery values.
        /// </summary>
        /// <param name="jsonApiContext"></param>
        /// <param name="filterQuery"></param>
        public AttrQuery(IJsonApiContext jsonApiContext, FilterQuery filterQuery)
        {
            _jsonApiContext = jsonApiContext;
            Attribute = GetAttribute(filterQuery.Attribute);
            
            if (Attribute.IsFilterable == false)
                throw new JsonApiException(400, $"Filter is not allowed for attribute '{Attribute.PublicAttributeName}'.");

            IsAttributeOfRelationship = _isAttributeOfRelationship;
            PropertyValue = filterQuery.Value;
            FilterOperation = filterQuery.OperationType;
        }

        /// <summary>
        /// Build AttrQuery based on SortQuery values.
        /// </summary>
        /// <param name="jsonApiContext"></param>
        /// <param name="sortQuery"></param>
        public AttrQuery(IJsonApiContext jsonApiContext, SortQuery sortQuery)
        {
            _jsonApiContext = jsonApiContext;
            Attribute = GetAttribute(sortQuery.Attribute);
            
            if (Attribute.IsSortable == false)
                throw new JsonApiException(400, $"Sort is not allowed for attribute '{Attribute.PublicAttributeName}'.");

            IsAttributeOfRelationship = _isAttributeOfRelationship;
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
