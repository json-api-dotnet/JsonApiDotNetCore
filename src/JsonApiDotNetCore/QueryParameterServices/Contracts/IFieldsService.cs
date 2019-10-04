using System.Collections.Generic;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.QueryServices.Contracts
{
    /// <summary>
    /// Query service to access sparse field selection.
    /// </summary>
    public interface IFieldsService
    {
        /// <summary>
        /// Gets the list of targeted fields. In a relationship is supplied,
        /// gets the list of targeted fields for that relationship.
        /// </summary>
        /// <param name="relationship"></param>
        /// <returns></returns>
        List<AttrAttribute> Get(RelationshipAttribute relationship = null);
    }

    /// <summary>
    /// Internal interface to register sparse field selections when parsing query params internally.
    /// This is to prevent the registering method from being exposed to the developer.
    /// </summary>
    public interface IInternalFieldsQueryService
    {
        void Register(AttrAttribute selected, RelationshipAttribute relationship = null);
    }
}