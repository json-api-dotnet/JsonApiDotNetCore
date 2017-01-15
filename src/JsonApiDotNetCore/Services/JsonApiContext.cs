using JsonApiDotNetCore.Internal;

namespace JsonApiDotNetCore.Services
{
    public class JsonApiContext : IJsonApiContext
    {
        public ContextEntity RequestEntity { get; set; }
        public IContextGraph ContextGraph { get; set; }
    }
}
