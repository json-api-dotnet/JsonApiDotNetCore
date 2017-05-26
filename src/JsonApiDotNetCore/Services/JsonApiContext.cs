using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Configuration;
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
        public JsonApiContext(
            IContextGraph contextGraph,
            IHttpContextAccessor httpContextAccessor,
            JsonApiOptions options,
            IMetaBuilder metaBuilder,
            IGenericProcessorFactory genericProcessorFactory)
        {
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

        public IJsonApiContext ApplyContext<T>()
        {
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
    }
}
