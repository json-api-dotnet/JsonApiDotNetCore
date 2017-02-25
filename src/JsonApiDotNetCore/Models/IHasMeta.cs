using System.Collections.Generic;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.Models
{
    public interface IHasMeta
    {
        Dictionary<string, object> GetMeta(IJsonApiContext context);
    }
}
