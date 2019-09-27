using System.Collections.Generic;
using JsonApiDotNetCore.Internal.Query;
using JsonApiDotNetCore.Managers.Contracts;

namespace JsonApiDotNetCore.Services
{
    public interface IQueryComposer
    {
        string Compose(IRequestContext jsonApiContext);
    }

    public class QueryComposer : IQueryComposer
    {
        public string Compose(IRequestContext requestManager)
        {
            string result = "";
            if (requestManager != null && requestManager.QuerySet != null)
            {
                List<FilterQuery> filterQueries = requestManager.QuerySet.Filters;
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
            var operation = string.IsNullOrWhiteSpace(query.Operation) ? query.Operation : query.Operation + ":";
            result += QueryConstants.OPEN_BRACKET + query.Attribute + QueryConstants.CLOSE_BRACKET + "=" + operation + query.Value;
            return result;
        }
    }
}
