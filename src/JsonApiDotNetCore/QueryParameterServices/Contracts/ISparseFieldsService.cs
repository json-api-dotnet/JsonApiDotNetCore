using System.Collections.Generic;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Query
{
    /// <summary>
    /// Query service to access sparse field selection.
    /// </summary>
    public interface ISparseFieldsService : IQueryParameterService
    {
        /// <summary>
        /// Gets the list of targeted fields. In a relationship is supplied,
        /// gets the list of targeted fields for that relationship.
        /// </summary>
        /// <param name="relationship"></param>
        List<AttrAttribute> Get(RelationshipAttribute relationship = null);
    }
}