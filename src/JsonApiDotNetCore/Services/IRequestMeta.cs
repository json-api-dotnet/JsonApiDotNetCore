using System.Collections.Generic;

namespace JsonApiDotNetCore.Services
{
    public interface IRequestMeta
    {
        Dictionary<string, object> GetMeta();
    }
}