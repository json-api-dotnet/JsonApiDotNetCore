using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Internal.Generics;
using JsonApiDotNetCore.Internal.Query;
using JsonApiDotNetCore.Managers.Contracts;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Services
{
    public interface IJsonApiApplication
    {
        IJsonApiOptions Options { get; set; }
        [Obsolete("Use standalone resourcegraph")]
        IResourceGraph ResourceGraph { get; set; }
    }

    public interface IQueryRequest
    {
        List<string> IncludedRelationships { get; set; }
        QuerySet QuerySet { get; set; }
        PageQueryService PageManager { get; set; }
    }

    public interface IJsonApiRequest : IJsonApiApplication,  IQueryRequest
    {
        /// <summary>
        /// If the request is a bulk json:api v1.1 operations request.
        /// This is determined by the `
        /// <see cref="JsonApiDotNetCore.Serialization.JsonApiDeserializer" />` class.
        /// 
        /// See [json-api/1254](https://github.com/json-api/json-api/pull/1254) for details.
        /// </summary>
        bool IsBulkOperationRequest { get; set; }

        /// <summary>
        /// The `<see cref="ContextEntity" />`for the target route.
        /// </summary>
        /// 
        /// <example>
        /// For a `GET /articles` request, `RequestEntity` will be set
        /// to the `Article` resource representation on the `JsonApiContext`.
        /// </example>
        ContextEntity RequestEntity { get; set; }

        /// <summary>
        /// The concrete type of the controller that was activated by the MVC routing middleware
        /// </summary>
        Type ControllerType { get; set; }

        /// <summary>
        /// The json:api meta data at the document level
        /// </summary>
        Dictionary<string, object> DocumentMeta { get; set; }

        /// <summary>
        /// If the request is on the `{id}/relationships/{relationshipName}` route
        /// </summary>
        bool IsRelationshipPath { get; }
    }

    public interface IJsonApiContext : IJsonApiRequest
    {
        [Obsolete("Use standalone IRequestManager")]
        IRequestManager RequestManager { get; set; }
        [Obsolete("Use standalone IPageManager")]
        IPageQueryService PageManager { get; set; }
        IJsonApiContext ApplyContext<T>(object controller);
        //IMetaBuilder MetaBuilder { get; set; }
        IGenericProcessorFactory GenericProcessorFactory { get; set; }
    }
}
