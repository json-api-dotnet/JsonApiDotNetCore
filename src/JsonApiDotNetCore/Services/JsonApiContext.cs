using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Internal;
using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCore.Services
{
    public class JsonApiContext : IJsonApiContext
    {
        private IHttpContextAccessor _httpContextAccessor;
        public JsonApiContext(
            IContextGraph contextGraph,
            IHttpContextAccessor httpContextAccessor)
        {
            ContextGraph = contextGraph;
            _httpContextAccessor = httpContextAccessor;
        }

        public IContextGraph ContextGraph { get; set; }
        public ContextEntity RequestEntity { get; set; }
        public string BasePath { get; set; }
        public IQueryCollection Query { get; set; }

        public IJsonApiContext ApplyContext<T>()
        {
            var context = _httpContextAccessor.HttpContext;

            RequestEntity = ContextGraph.GetContextEntity(typeof(T));
            
            Query = context.Request.Query;

            var linkBuilder = new LinkBuilder(this);
            BasePath = linkBuilder.GetBasePath(context, RequestEntity.EntityName);
            
            return this;
        }
    }
}
