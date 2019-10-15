using System.Collections.Generic;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Query
{
    /// <summary>
    /// Query parameter service responsible for url queries of the form ?include=X.Y.Z,U.V.W
    /// </summary>
    public interface IIncludeService : IQueryParameterService
    {
        /// <summary>
        /// Gets the parsed relationship inclusion chains.
        /// </summary>
        List<List<RelationshipAttribute>> Get();
    }
}