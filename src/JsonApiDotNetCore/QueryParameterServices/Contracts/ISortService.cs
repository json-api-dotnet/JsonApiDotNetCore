using System.Collections.Generic;
using JsonApiDotNetCore.Internal.Query;

namespace JsonApiDotNetCore.Query
{
    /// <summary>
    /// Query parameter service responsible for url queries of the form ?sort=-X
    /// </summary>
    public interface ISortService : IQueryParameterService
    {
        /// <summary>
        /// Gets the parsed sort queries
        /// </summary>
        List<SortQueryContext> Get();
    }
}