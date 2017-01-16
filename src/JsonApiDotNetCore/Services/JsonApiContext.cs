using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Internal;
using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCore.Services
{
    public class JsonApiContext : IJsonApiContext
    {
        public JsonApiContext(IContextGraph contextGraph)
        {
            ContextGraph = contextGraph;
        }

        public IContextGraph ContextGraph { get; set; }
        public ContextEntity RequestEntity { get; set; }
        public string BasePath { get; set; }
        public IQueryCollection Query { get; set; }

        public void ApplyContext(HttpContext context)
        {
            var linkBuilder = new LinkBuilder(this);
            BasePath = linkBuilder.GetBasePath(context, RequestEntity.EntityName);
            Query = context.Request.Query;
        }        
    }
}
