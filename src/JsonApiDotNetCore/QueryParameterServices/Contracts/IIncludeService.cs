using System.Collections.Generic;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Query
{
    /// <summary>
    /// Query service to access the inclusion chains.
    /// </summary>
    public interface IIncludeService : IQueryParameterService
    {
        /// <summary>
        /// Gets the list of included relationships chains for the current request.
        /// </summary>
        List<List<RelationshipAttribute>> Get();
    }
}