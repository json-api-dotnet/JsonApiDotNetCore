using System.Linq;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.Internal.Query
{
    public class RelatedAttrFilterQuery : RelatedAttrQuery
    {
       
        public RelatedAttrFilterQuery(
            IJsonApiContext jsonApiContext,
            FilterQuery filterQuery)
            :base(jsonApiContext, filterQuery)
        {
            PropertyValue = filterQuery.Value;
            FilterOperation = filterQuery.OperationType;
        }

        public string PropertyValue { get; set; }
        public FilterOperations FilterOperation { get; set; }

    }
}
