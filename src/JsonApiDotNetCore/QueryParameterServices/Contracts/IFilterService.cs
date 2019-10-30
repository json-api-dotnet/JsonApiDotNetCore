using System.Collections.Generic;
using JsonApiDotNetCore.Internal.Query;

namespace JsonApiDotNetCore.Query
{
    /// <summary>
    /// Query parameter service responsible for url queries of the form ?filter[X]=Y
    /// </summary>
    public interface IFilterService : IQueryParameterService
    {
        /// <summary>
        /// Gets the parsed filter queries
        /// </summary>
        List<FilterQueryContext> Get();
    }
}