using JsonApiDotNetCore.Internal;

namespace JsonApiDotNetCore.Services
{
    public interface IJsonApiContext
    {
        IContextGraph ContextGraph { get; set; }
        ContextEntity RequestEntity { get; set; }
    }
}
