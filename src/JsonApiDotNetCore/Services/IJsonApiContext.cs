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
using JsonApiDotNetCore.Request;

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
        PageManager PageManager { get; set; }
    }

    public interface IJsonApiRequest : IJsonApiApplication,  IQueryRequest
    {
        /// <summary>
        /// Stores information to set relationships for the request resource. 
        /// These relationships must already exist and should not be re-created.
        /// By default, it is the responsibility of the repository to use the 
        /// relationship pointers to persist the relationship.
        /// 
        /// The expected use case is POST-ing or PATCH-ing an entity with HasMany 
        /// relationships:
        /// <code>
        /// {
        ///    "data": {
        ///      "type": "photos",
        ///      "attributes": {
        ///        "title": "Ember Hamster",
        ///        "src": "http://example.com/images/productivity.png"
        ///      },
        ///      "relationships": {
        ///        "tags": {
        ///          "data": [
        ///            { "type": "tags", "id": "2" },
        ///            { "type": "tags", "id": "3" }
        ///          ]
        ///        }
        ///      }
        ///    }
        ///  }
        /// </code>
        /// </summary>
        HasManyRelationshipPointers HasManyRelationshipPointers { get; }

        /// <summary>
        /// Stores information to set relationships for the request resource. 
        /// These relationships must already exist and should not be re-created.
        /// 
        /// The expected use case is POST-ing or PATCH-ing 
        /// an entity with HasOne relationships:
        /// <code>
        /// {
        ///    "data": {
        ///      "type": "photos",
        ///      "attributes": {
        ///        "title": "Ember Hamster",
        ///        "src": "http://example.com/images/productivity.png"
        ///      },
        ///      "relationships": {
        ///        "photographer": {
        ///          "data": { "type": "people", "id": "2" }
        ///        }
        ///      }
        ///    }
        ///  }
        /// </code>
        /// </summary>
        HasOneRelationshipPointers HasOneRelationshipPointers { get; }

        /// <summary>
        /// If the request is a bulk json:api v1.1 operations request.
        /// This is determined by the `
        /// <see cref="JsonApiDotNetCore.Serialization.JsonApiDeSerializer" />` class.
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
        IPageManager PageManager { get; set; }
        IJsonApiContext ApplyContext<T>(object controller);
        IMetaBuilder MetaBuilder { get; set; }
        IGenericProcessorFactory GenericProcessorFactory { get; set; }

        /// <summary>
        /// **_Experimental_**: do not use. It is likely to change in the future.
        /// 
        /// Resets operational state information.
        /// </summary>
        void BeginOperation();
    }
}
