using JsonApiDotNetCore.Internal;

namespace JsonApiDotNetCore.Services
{
    public class JsonApiContext : IJsonApiContext
    {
        public IContextGraph ContextGraph { get; set; }
    }
}
