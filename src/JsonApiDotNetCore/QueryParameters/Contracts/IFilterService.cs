using System.Collections.Generic;
using JsonApiDotNetCore.Internal.Query;

namespace JsonApiDotNetCore.Query
{
    public interface IFilterService : IParsableQueryParameter
    {
        List<FilterQueryContext> Get();
    }
}