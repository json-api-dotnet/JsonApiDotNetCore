using System.Collections.Generic;
using JsonApiDotNetCore.Models.Annotation;

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
        List<AttrAttribute> Get(RelationshipAttribute relationship = null);

        /// <summary>
        /// Gets the set of all targeted fields, including fields for related entities, as a set of dotted property names.
        /// </summary>
        ISet<string> GetAll();
    }
}
