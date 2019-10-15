using System.Collections.Generic;
using JsonApiDotNetCore.Internal.Query;

namespace JsonApiDotNetCore.Query
{
    public interface ISortService : IQueryParameterService
    {
        List<SortQueryContext> Get();
    }
}