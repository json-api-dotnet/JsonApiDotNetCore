using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Generics;
using JsonApiDotNetCore.Internal.Query;
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
            IContextGraph contextGraph,
            IHttpContextAccessor httpContextAccessor,
            JsonApiOptions options,
            IMetaBuilder metaBuilder,
            IGenericProcessorFactory genericProcessorFactory,
            IQueryParser queryParser,
            IControllerContext controllerContext)
        {
            ContextGraph = contextGraph;
            _httpContextAccessor = httpContextAccessor;
            Options = options;
            MetaBuilder = metaBuilder;
            GenericProcessorFactory = genericProcessorFactory;
            _queryParser = queryParser;
            _controllerContext = controllerContext;
        }

        public JsonApiOptions Options { get; set; }
        public IContextGraph ContextGraph { get; set; }
        [Obsolete("Use the proxied member IControllerContext.RequestEntity instead.")]
        public ContextEntity RequestEntity { get => _controllerContext.RequestEntity; set => _controllerContext.RequestEntity = value; }
        public string BasePath { get; set; }
        public QuerySet QuerySet { get; set; }
        public bool IsRelationshipData { get; set; }
        public bool IsRelationshipPath { get; private set; }
        public List<string> IncludedRelationships { get; set; }
        public PageManager PageManager { get; set; }
        public IMetaBuilder MetaBuilder { get; set; }
        public IGenericProcessorFactory GenericProcessorFactory { get; set; }
        public Type ControllerType { get; set; }
        public Dictionary<string, object> DocumentMeta { get; set; }
        public bool IsBulkOperationRequest { get; set; }

        public Dictionary<AttrAttribute, object> AttributesToUpdate { get; set; } = new Dictionary<AttrAttribute, object>();
        public Dictionary<RelationshipAttribute, object> RelationshipsToUpdate { get; set; } = new Dictionary<RelationshipAttribute, object>();
        public HasManyRelationshipPointers HasManyRelationshipPointers { get; private set; } = new HasManyRelationshipPointers();
        public HasOneRelationshipPointers HasOneRelationshipPointers { get; private set; } = new HasOneRelationshipPointers();

        public IJsonApiContext ApplyContext<T>(object controller)
        {
            if (controller == null)
                throw new JsonApiException(500, $"Cannot ApplyContext from null controller for type {typeof(T)}");

            _controllerContext.ControllerType = controller.GetType();
            _controllerContext.RequestEntity = ContextGraph.GetContextEntity(typeof(T));
            if (_controllerContext.RequestEntity == null)
                throw new JsonApiException(500, $"A resource has not been properly defined for type '{typeof(T)}'. Ensure it has been registered on the ContextGraph.");

            var context = _httpContextAccessor.HttpContext;

            if (context.Request.Query.Count > 0)
            {
                QuerySet = _queryParser.Parse(context.Request.Query);
                IncludedRelationships = QuerySet.IncludedRelationships;
            }

            BasePath = new LinkBuilder(this).GetBasePath(context, _controllerContext.RequestEntity.EntityName);
            PageManager = GetPageManager();
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

        private PageManager GetPageManager()
        {
            if (Options.DefaultPageSize == 0 && (QuerySet == null || QuerySet.PageQuery.PageSize == 0))
                return new PageManager();

            var query = QuerySet?.PageQuery ?? new PageQuery();

            return new PageManager
            {
                DefaultPageSize = Options.DefaultPageSize,
                CurrentPage = query.PageOffset,
                PageSize = query.PageSize > 0 ? query.PageSize : Options.DefaultPageSize
            };
        }

        [Obsolete("Use the proxied method IControllerContext.GetControllerAttribute instead.")]
        public TAttribute GetControllerAttribute<TAttribute>() where TAttribute : Attribute
            => _controllerContext.GetControllerAttribute<TAttribute>();

        public void BeginOperation()
        {
            IncludedRelationships = new List<string>();
            AttributesToUpdate = new Dictionary<AttrAttribute, object>();
            RelationshipsToUpdate = new Dictionary<RelationshipAttribute, object>();
            HasManyRelationshipPointers = new HasManyRelationshipPointers();
            HasOneRelationshipPointers = new HasOneRelationshipPointers();
        }
    }
}
