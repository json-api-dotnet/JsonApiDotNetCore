using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Graph;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Links;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Serialization.Contracts;

using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Configuration
{

    /// <summary>
    /// Global options
    /// </summary>
    public class JsonApiOptions : IJsonApiOptions
    {
        /// <summary>
        /// Use relative links for all resources.
        /// </summary>
        /// <example>
        /// <code>
        /// options.RelativeLinks = true;
        /// </code>
        /// <code>
        /// {
        ///   "type": "articles",
        ///   "id": "4309",
        ///   "relationships": {
        ///      "author": {
        ///        "links": {
        ///          "self": "/api/v1/articles/4309/relationships/author",
        ///          "related": "/api/v1/articles/4309/author"
        ///        }
        ///      }
        ///   }
        /// }
        /// </code>
        /// </example>
        public bool RelativeLinks { get; set; } = false;

        /// <summary>
        /// Configures globally which links to show in the <see cref="RelationshipLinks"/>
        /// object for a requested resource. Setting can be overriden per resource by
        /// setting the <see cref="ContextEntity.RelationshipLinks"/> option or on the
        /// RelationshipAttribute in the class definition of your model.
        /// </summary>
        /// <example>
        /// <code>
        /// options.DefaultRelationshipLinks = Link.None;
        /// </code>
        /// <code>
        /// {
        ///   "type": "articles",
        ///   "id": "4309",
        ///   "relationships": {
        ///      "author": { "data": { "type": "people", "id": "1234" }
        ///      }
        ///   }
        /// }
        /// </code>
        /// </example>
        public Link RelationshipLinks { get; set; } = Link.All;


        /// <summary>
        /// Configures globally which links to show in the <see cref="TopLevelLinks"/>
        /// object for a requested resource. Setting can be overriden per resource by
        /// setting the <see cref="ContextEntity.TopLevelLinks"/> option.
        /// </summary>
        public Link TopLevelLinks { get; set; } = Link.All;


        /// <summary>
        /// Configures globally which links to show in the <see cref="ResourceLinks"/>
        /// object for a requested resource. Setting can be overriden per resource by
        /// setting the <see cref="ContextEntity.ResourceLinks"/> option.
        /// </summary>
        public Link ResourceLinks { get; set; } = Link.All;

        /// <summary>
        /// Provides an interface for formatting resource names by convention
        /// </summary>
        public static IResourceNameFormatter ResourceNameFormatter { get; set; } = new DefaultResourceNameFormatter();

        /// <summary>
        /// Provides an interface for formatting relationship id properties given the navigation property name
        /// </summary>
        public static IRelatedIdMapper RelatedIdMapper { get; set; } = new DefaultRelatedIdMapper();

        /// <summary>
        /// Whether or not stack traces should be serialized in Error objects
        /// </summary>
        public static bool DisableErrorStackTraces { get; set; }

        /// <summary>
        /// Whether or not source URLs should be serialized in Error objects
        /// </summary>
        public static bool DisableErrorSource { get; set; }

        /// <summary>
        /// Whether or not ResourceHooks are enabled. 
        /// 
        /// Default is set to <see langword="false"/> for backward compatibility.
        /// </summary>
        public bool EnableResourceHooks { get; set; } = false;

        /// <summary>
        /// Whether or not database values should be included by default
        /// for resource hooks. Ignored if EnableResourceHooks is set false.
        /// 
        /// Defaults to <see langword="false"/>.
        /// </summary>
        public bool LoaDatabaseValues { get; set; } = false;

        /// <summary>
        /// The base URL Namespace
        /// </summary>
        /// <example>
        /// <code>options.Namespace = "api/v1";</code>
        /// </example>
        public string Namespace { get; set; }

        /// <summary>
        /// The default page size for all resources
        /// </summary>
        /// <example>
        /// <code>options.DefaultPageSize = 10;</code>
        /// </example>
        public int DefaultPageSize { get; set; }

        /// <summary>
        /// Whether or not the total-record count should be included in all document
        /// level meta objects.
        /// Defaults to false.
        /// </summary>
        /// <example>
        /// <code>options.IncludeTotalRecordCount = true;</code>
        /// </example>
        public bool IncludeTotalRecordCount { get; set; }

        /// <summary>
        /// Whether or not clients can provide ids when creating resources.
        /// Defaults to false.  When disabled the application will respond 
        /// with a 403 Forbidden respponse if a client attempts to create a 
        /// resource with a defined id.
        /// </summary>
        /// <example>
        /// <code>options.AllowClientGeneratedIds = true;</code>
        /// </example>
        public bool AllowClientGeneratedIds { get; set; }

        /// <summary>
        /// The graph of all resources exposed by this application.
        /// </summary>
        [Obsolete("Use the standalone resourcegraph")]
        public IResourceGraph ResourceGraph { get; set; }

        /// <summary>
        /// Whether or not to allow all custom query parameters.
        /// </summary>
        /// <example>
        /// <code>
        /// options.AllowCustomQueryParameters = true;
        /// </code>
        /// </example>
        public bool AllowCustomQueryParameters { get; set; }

        /// <summary>
        /// The default behavior for serializing null attributes.
        /// </summary>
        /// <example>
        /// <code>
        /// options.NullAttributeResponseBehavior = new NullAttributeResponseBehavior {
        ///  // ...
        ///};
        /// </code>
        /// </example>
        public NullAttributeResponseBehavior NullAttributeResponseBehavior { get; set; }

        /// <summary>
        /// Whether or not to allow json:api v1.1 operation requests.
        /// This is a beta feature and there may be breaking changes
        /// in subsequent releases. For now, ijt should be considered
        /// experimental.
        /// </summary>
        /// <remarks>
        /// This will be enabled by default in a subsequent patch JsonApiDotNetCore v2.2.x
        /// </remarks>
        public bool EnableOperations { get; set; }

        /// <summary>
        /// Whether or not to validate model state.
        /// </summary>
        /// <example>
        /// <code>
        /// options.ValidateModelState = true;
        /// </code>
        /// </example>
        public bool ValidateModelState { get; set; }

        public JsonSerializerSettings SerializerSettings { get; } = new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new DasherizedResolver()
        };

        public void BuildResourceGraph<TContext>(Action<IResourceGraphBuilder> builder) where TContext : DbContext
        {
            BuildResourceGraph(builder);

            ResourceGraphBuilder.AddDbContext<TContext>();

            ResourceGraph = ResourceGraphBuilder.Build();
        }

        public void BuildResourceGraph(Action<IResourceGraphBuilder> builder)
        {
            if (builder == null) return;

            builder(ResourceGraphBuilder);

            ResourceGraph = ResourceGraphBuilder.Build();
        }

        public void EnableExtension(JsonApiExtension extension)
            => EnabledExtensions.Add(extension);

        internal IResourceGraphBuilder ResourceGraphBuilder { get; } = new ResourceGraphBuilder();
        internal List<JsonApiExtension> EnabledExtensions { get; set; } = new List<JsonApiExtension>();
    }
}
