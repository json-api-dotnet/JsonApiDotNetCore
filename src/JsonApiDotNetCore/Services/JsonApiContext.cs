using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Generics;
using JsonApiDotNetCore.Internal.Query;
using JsonApiDotNetCore.Models;
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
        public Dictionary<AttrAttribute, object> AttributesToUpdate { get; set; } = new Dictionary<AttrAttribute, object>();
        public Dictionary<RelationshipAttribute, object> RelationshipsToUpdate { get; set; } = new Dictionary<RelationshipAttribute, object>();
        public Type ControllerType { get; set; }
        public Dictionary<string, object> DocumentMeta { get; set; }
        public bool IsBulkOperationRequest { get; set; }

        public IJsonApiContext ApplyContext<T>(object controller)
        {
            if (controller == null)
                throw new JsonApiException(500, $"Cannot ApplyContext from null controller for type {typeof(T)}");

            _controllerContext.ControllerType = controller.GetType();
            _controllerContext.RequestEntity = ContextGraph.GetContextEntity(typeof(T));
            if (_controllerContext.RequestEntity == null)
                throw new JsonApiException(500, $"A resource has not been properly defined for type '{typeof(T)}'. Ensure it has been registered on the ContextGraph.");

            var context = _httpContextAccessor.HttpContext;
            var requestPath = context.Request.Path.Value;

            if (context.Request.Query.Count > 0)
            {
                QuerySet = _queryParser.Parse(context.Request.Query);
                IncludedRelationships = QuerySet.IncludedRelationships;
            }

            var linkBuilder = new LinkBuilder(this);
            BasePath = linkBuilder.GetBasePath(context, _controllerContext.RequestEntity.EntityName);
            PageManager = GetPageManager();

            var pathSpans = requestPath.SpanSplit('/');
            IsRelationshipPath = pathSpans[pathSpans.Count - 2].ToString() == "relationships";

            return this;
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
    }
}
