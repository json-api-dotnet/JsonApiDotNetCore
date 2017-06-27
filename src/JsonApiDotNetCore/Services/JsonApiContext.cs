using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Data;
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
        private readonly IDbContextResolver _contextResolver;

        public JsonApiContext(
            IDbContextResolver contextResolver,
            IContextGraph contextGraph,
            IHttpContextAccessor httpContextAccessor,
            JsonApiOptions options,
            IMetaBuilder metaBuilder,
            IGenericProcessorFactory genericProcessorFactory)
        {
            _contextResolver = contextResolver;
            ContextGraph = contextGraph;
            _httpContextAccessor = httpContextAccessor;
            Options = options;
            MetaBuilder = metaBuilder;
            GenericProcessorFactory = genericProcessorFactory;
        }

        public JsonApiOptions Options { get; set; }
        public IContextGraph ContextGraph { get; set; }
        public ContextEntity RequestEntity { get; set; }
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

        public IJsonApiContext ApplyContext<T>(object controller)
        {
            if (controller == null)
                throw new JsonApiException(500, $"Cannot ApplyContext from null controller for type {typeof(T)}");

            ControllerType = controller.GetType();

            var context = _httpContextAccessor.HttpContext;
            var path = context.Request.Path.Value.Split('/');

            RequestEntity = ContextGraph.GetContextEntity(typeof(T));

            if (context.Request.Query.Any())
            {
                QuerySet = new QuerySet(this, context.Request.Query);
                IncludedRelationships = QuerySet.IncludedRelationships;
            }

            var linkBuilder = new LinkBuilder(this);
            BasePath = linkBuilder.GetBasePath(context, RequestEntity.EntityName);
            PageManager = GetPageManager();
            IsRelationshipPath = path[path.Length - 2] == "relationships";
            return this;
        }

        public IDbContextResolver GetDbContextResolver() => _contextResolver;

        private PageManager GetPageManager()
        {
            if (Options.DefaultPageSize == 0 && (QuerySet == null || QuerySet.PageQuery.PageSize == 0))
                return new PageManager();

            var query = QuerySet?.PageQuery ?? new PageQuery();

            return new PageManager
            {
                DefaultPageSize = Options.DefaultPageSize,
                CurrentPage = query.PageOffset > 0 ? query.PageOffset : 1,
                PageSize = query.PageSize > 0 ? query.PageSize : Options.DefaultPageSize
            };
        }

        public TAttribute GetControllerAttribute<TAttribute>() where TAttribute : Attribute
        {
            var attribute = ControllerType.GetTypeInfo().GetCustomAttribute(typeof(TAttribute));
            return attribute == null ? null : (TAttribute)attribute;
        }
    }
}
