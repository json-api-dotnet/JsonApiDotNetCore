using System;
using System.Collections.Generic;
using System.Text;

namespace JsonApiDotNetCore.Managers.Contracts
{
    public interface IQueryManager
    {
        /// <summary>
        /// Gets the relationships as set in the query parameters
        /// </summary>
        /// <returns></returns>
        List<string> GetRelationships();
        /// <summary>
        /// Gets the sparse fields
        /// </summary>
        /// <returns></returns>
        List<string> GetFields();
    }
}
