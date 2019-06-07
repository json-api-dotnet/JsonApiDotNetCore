using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Generics;
using JsonApiDotNetCore.Internal.Query;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Request;

namespace JsonApiDotNetCore.Services
{
    public interface IJsonApiApplication
    {
        JsonApiOptions Options { get; set; }
        IResourceGraph ResourceGraph { get; set; }
    }

    public interface IQueryRequest
    {
        List<string> IncludedRelationships { get; set; }
        QuerySet QuerySet { get; set; }
        PageManager PageManager { get; set; }
    }

    public interface IUpdateRequest
    {
        /// <summary>
        /// The attributes that were included in a PATCH request. 
        /// Only the attributes in this dictionary should be updated.
        /// </summary>
        Dictionary<AttrAttribute, object> AttributesToUpdate { get; set; }

        /// <summary>
        /// Any relationships that were included in a PATCH request. 
        /// Only the relationships in this dictionary should be updated.
        /// </summary>
        Dictionary<RelationshipAttribute, object> RelationshipsToUpdate { get; }
    }

    public interface IJsonApiRequest : IJsonApiApplication, IUpdateRequest, IQueryRequest
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

        /// <summary>
        /// Stores information to set relationships for the request resource. 
        /// These relationships must already exist and should not be re-created.
        /// By default, it is the responsibility of the repository to use the 
        /// relationship pointers to persist the relationship.
        /// 
        /// The expected use case is POST-ing or PATCH-ing an entity with HasMany 
        /// relaitonships:
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

        [Obsolete("Use `IsRelationshipPath` instead.")]
        bool IsRelationshipData { get; set; }
    }

    public interface IJsonApiContext : IJsonApiRequest
    {
        IJsonApiContext ApplyContext<T>(object controller);
        IMetaBuilder MetaBuilder { get; set; }
        IGenericProcessorFactory GenericProcessorFactory { get; set; }

        [Obsolete("Use the proxied method IControllerContext.GetControllerAttribute instead.")]
        TAttribute GetControllerAttribute<TAttribute>() where TAttribute : Attribute;

        /// <summary>
        /// **_Experimental_**: do not use. It is likely to change in the future.
        /// 
        /// Resets operational state information.
        /// </summary>
        void BeginOperation();
    }
}
