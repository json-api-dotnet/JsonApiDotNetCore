using System.Collections.Generic;
using JsonApiDotNetCore.Internal.Query;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Services
{
    public interface IQueryComposer
    {
        string Compose(IJsonApiContext jsonApiContext);
    }

    public class QueryComposer : IQueryComposer
    {
        public string Compose(IJsonApiContext jsonApiContext)
        {
            string result = "";
            if(jsonApiContext != null && jsonApiContext.QuerySet != null)
            {                
                List<FilterQuery> filterQueries = jsonApiContext.QuerySet.Filters;
                if (filterQueries.Count > 0)
                {
                    foreach (FilterQuery filter in filterQueries)
                    {
                        result += ComposeSingleFilter(filter);
                    }
                }
            }
            return result;
        }

        private string ComposeSingleFilter(FilterQuery query)
        {
            var result = "&filter";
            result += QueryConstants.OPEN_BRACKET + query.Attribute + QueryConstants.CLOSE_BRACKET + query.Operation + query.Value;
            return result;
        }
    }
}
