using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace JsonApiDotNetCore.Services
{
    public class JsonApiContext : IJsonApiContext
    {
        RouteData _routeData;

        public JsonApiContext(IContextGraph contextGraph)
        {
            ContextGraph = contextGraph;
        }

        public IContextGraph ContextGraph { get; set; }
        public ContextEntity RequestEntity { get; set; }
        public string BasePath { get; set; }

        public void ApplyContext(HttpContext context)
        {
            var linkBuilder = new LinkBuilder(this);
            BasePath = linkBuilder.GetBasePath(context, RequestEntity.EntityName);
        }        
    }
}
