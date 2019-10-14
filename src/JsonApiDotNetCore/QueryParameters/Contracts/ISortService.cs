using System.Collections.Generic;
using JsonApiDotNetCore.Internal.Query;

namespace JsonApiDotNetCore.Query
{
    public interface ISortService
    {
        List<SortQueryContext> Get();
    }
}