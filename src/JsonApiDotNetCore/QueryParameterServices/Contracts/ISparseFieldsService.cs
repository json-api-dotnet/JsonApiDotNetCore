using System.Collections.Generic;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Query
{
    /// <summary>
    /// Query parameter service responsible for url queries of the form ?fields[X]=U,V,W
    /// </summary>
    public interface ISparseFieldsService : IQueryParameterService
    {
        /// <summary>
        /// Gets the list of targeted fields. If a relationship is supplied,
        /// gets the list of targeted fields for that relationship.
        /// </summary>
        /// <param name="relationship"></param>
        List<AttrAttribute> Get(RelationshipAttribute relationship = null);
    }
}