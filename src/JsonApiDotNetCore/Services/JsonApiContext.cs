using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Internal.Generics;
using JsonApiDotNetCore.Internal.Query;
using JsonApiDotNetCore.Managers.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Request;
using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCore.Services
{
    public class JsonApiContext : IJsonApiContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IQueryParser _queryParser;
        private readonly IControllerContext _controllerContext;

        public JsonApiContext(
            IResourceGraph resourceGraph,
            IHttpContextAccessor httpContextAccessor,
            IJsonApiOptions options,
            IMetaBuilder metaBuilder,
            IGenericProcessorFactory genericProcessorFactory,
            IQueryParser queryParser,
            IPageManager pageManager,
            IRequestManager requestManager,
            IControllerContext controllerContext)
        {
            RequestManager = requestManager;
            PageManager = pageManager;
            ResourceGraph = resourceGraph;
            _httpContextAccessor = httpContextAccessor;
            Options = options;
            MetaBuilder = metaBuilder;
            GenericProcessorFactory = genericProcessorFactory;
            _queryParser = queryParser;
            _controllerContext = controllerContext;
        }

        public IJsonApiOptions Options { get; set; }
        [Obsolete("Please use the standalone `IResourceGraph`")]
        public IResourceGraph ResourceGraph { get; set; }
        [Obsolete("Use the proxied member IControllerContext.RequestEntity instead.")]
        public ContextEntity RequestEntity { get => _controllerContext.RequestEntity; set => _controllerContext.RequestEntity = value; }

        [Obsolete("Please us the IRequestManager")]
        public QuerySet QuerySet { get; set; }
        public bool IsRelationshipData { get; set; }
        public bool IsRelationshipPath { get; private set; }
        public List<string> IncludedRelationships { get; set; }
        public IPageManager PageManager { get; set; }
        public IMetaBuilder MetaBuilder { get; set; }
        public IGenericProcessorFactory GenericProcessorFactory { get; set; }
        public Type ControllerType { get; set; }
        public Dictionary<string, object> DocumentMeta { get; set; }
        public bool IsBulkOperationRequest { get; set; }

        public Dictionary<AttrAttribute, object> AttributesToUpdate { get; set; } = new Dictionary<AttrAttribute, object>();
        public Dictionary<RelationshipAttribute, object> RelationshipsToUpdate { get => GetRelationshipsToUpdate(); }

        private Dictionary<RelationshipAttribute, object> GetRelationshipsToUpdate()
        {
            var hasOneEntries = HasOneRelationshipPointers.Get().ToDictionary(kvp => (RelationshipAttribute)kvp.Key, kvp => (object)kvp.Value);
            var hasManyEntries = HasManyRelationshipPointers.Get().ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);
            return hasOneEntries.Union(hasManyEntries).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public HasManyRelationshipPointers HasManyRelationshipPointers { get; private set; } = new HasManyRelationshipPointers();
        public HasOneRelationshipPointers HasOneRelationshipPointers { get; private set; } = new HasOneRelationshipPointers();
        [Obsolete("Please use the standalone Requestmanager")]
        public IRequestManager RequestManager { get; set; }
        PageManager IQueryRequest.PageManager { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        [Obsolete("This is no longer necessary")]
        public IJsonApiContext ApplyContext<T>(object controller)
        {
            if (controller == null)
                throw new JsonApiException(500, $"Cannot ApplyContext from null controller for type {typeof(T)}");

            _controllerContext.ControllerType = controller.GetType();
            _controllerContext.RequestEntity = ResourceGraph.GetContextEntity(typeof(T));
            if (_controllerContext.RequestEntity == null)
                throw new JsonApiException(500, $"A resource has not been properly defined for type '{typeof(T)}'. Ensure it has been registered on the ResourceGraph.");

            var context = _httpContextAccessor.HttpContext;

            if (context.Request.Query.Count > 0)
            {
                QuerySet = _queryParser.Parse(context.Request.Query);
                IncludedRelationships = QuerySet.IncludedRelationships;
            }

            IsRelationshipPath = PathIsRelationship(context.Request.Path.Value);

            return this;
        }

        internal static bool PathIsRelationship(string requestPath)
        {
            // while(!Debugger.IsAttached) { Thread.Sleep(1000); }
            const string relationships = "relationships";
            const char pathSegmentDelimiter = '/';

            var span = requestPath.AsSpan();

            // we need to iterate over the string, from the end,
            // checking whether or not the 2nd to last path segment
            // is "relationships"
            // -2 is chosen in case the path ends with '/'
            for (var i = requestPath.Length - 2; i >= 0; i--)
            {
                // if there are not enough characters left in the path to 
                // contain "relationships"
                if (i < relationships.Length)
                    return false;

                // we have found the first instance of '/'
                if (span[i] == pathSegmentDelimiter)
                {
                    // in the case of a "relationships" route, the next
                    // path segment will be "relationships"
                    return (
                        span.Slice(i - relationships.Length, relationships.Length)
                            .SequenceEqual(relationships.AsSpan())
                    );
                }
            }

            return false;
        }

        public void BeginOperation()
        {
            IncludedRelationships = new List<string>();
            AttributesToUpdate = new Dictionary<AttrAttribute, object>();
            HasManyRelationshipPointers = new HasManyRelationshipPointers();
            HasOneRelationshipPointers = new HasOneRelationshipPointers();
        }
    }
}
