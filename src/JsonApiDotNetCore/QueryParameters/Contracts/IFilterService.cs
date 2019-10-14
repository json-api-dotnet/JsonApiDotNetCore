using System.Collections.Generic;
using JsonApiDotNetCore.Internal.Query;

namespace JsonApiDotNetCore.Query
{
    public interface IFilterService, IQueryParameterService
    {
        List<FilterQueryContext> Get();
    }
}