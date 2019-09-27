using System.Collections.Generic;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Query;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCore.Managers.Contracts
{
    public interface ICurrentRequest : IQueryRequest
    {
        /// <summary>
        /// The request namespace. This may be an absolute or relative path
        /// depending upon the configuration.
        /// </summary>
        /// <example>
        /// Absolute: https://example.com/api/v1
        /// 
        /// Relative: /api/v1
        /// </example>
        string BasePath { get; set; }
        QuerySet QuerySet { get; set; }
        IQueryCollection FullQuerySet { get; set; }

        /// <summary>
        /// If the request is on the `{id}/relationships/{relationshipName}` route
        /// </summary>
        bool IsRelationshipPath { get; set; }
        /// <summary>
        /// Sets the current context entity for this entire request
        /// </summary>
        /// <param name="contextEntityCurrent"></param>
        void SetRequestResource(ContextEntity contextEntityCurrent);

        ContextEntity GetRequestResource();
        /// <summary>
        /// Which query params are filtered
        /// </summary>
        QueryParams DisabledQueryParams { get; set; }
        bool IsBulkRequest { get; set; }

    }
}
