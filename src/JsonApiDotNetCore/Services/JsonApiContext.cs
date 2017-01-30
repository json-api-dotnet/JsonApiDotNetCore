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
        public List<string> IncludedRelationships { get; set; }

        public IJsonApiContext ApplyContext<T>()
        {
            var context = _httpContextAccessor.HttpContext;

            RequestEntity = ContextGraph.GetContextEntity(typeof(T));
            
            if(context.Request.Query.Any())
            {
                QuerySet = new QuerySet(this, context.Request.Query);
                IncludedRelationships = QuerySet.IncludedRelationships;
            }                

            var linkBuilder = new LinkBuilder(this);
            BasePath = linkBuilder.GetBasePath(context, RequestEntity.EntityName);
            
            return this;
        }
    }
}
