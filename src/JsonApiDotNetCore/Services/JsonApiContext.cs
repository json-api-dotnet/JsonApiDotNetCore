using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Query;
using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCore.Services
{
    public class JsonApiContext : IJsonApiContext
    {
        private IHttpContextAccessor _httpContextAccessor;
        public JsonApiContext(
            IContextGraph contextGraph,
            IHttpContextAccessor httpContextAccessor,
            JsonApiOptions options)
        {
            ContextGraph = contextGraph;
            _httpContextAccessor = httpContextAccessor;
            Options = options;
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

        public IJsonApiContext ApplyContext<T>()
        {
            var context = _httpContextAccessor.HttpContext;
            var path = context.Request.Path.Value.Split('/');

            RequestEntity = ContextGraph.GetContextEntity(typeof(T));
            
            if(context.Request.Query.Any())
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
            if(Options.DefaultPageSize == 0 && (QuerySet == null || QuerySet.PageQuery.PageSize == 0))
                return new PageManager();
            
            var query = QuerySet?.PageQuery ?? new PageQuery(); 

            return new PageManager {
                DefaultPageSize = Options.DefaultPageSize,
                CurrentPage = query.PageOffset > 0 ? query.PageOffset : 1,
                PageSize = query.PageSize > 0 ? query.PageSize : Options.DefaultPageSize
            };
        }
    }
}
